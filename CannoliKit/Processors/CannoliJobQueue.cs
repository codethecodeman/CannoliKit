using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Processors.Channels;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Timer = System.Timers.Timer;

namespace CannoliKit.Processors;

public sealed class CannoliJobQueue<TJob> : ICannoliJobQueue<TJob>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICannoliWorkerChannel<TJob> _channel;
    private readonly ConcurrentBag<Timer> _repeatingWorkTimers;
    private readonly SemaphoreSlim _taskSemaphore;
    private readonly int _maxConcurrentJobsCount;
    private bool _isRunning, _isDisposed;
    internal delegate Task LogEventHandler(LogMessage e);
    internal event LogEventHandler? Log;

    internal CannoliJobQueue(
        IServiceProvider serviceProvider,
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = serviceScopeFactory;
        _channel = new PriorityChannel<TJob>();



        _maxConcurrentJobsCount = attribute?.MaxConcurrentJobs ?? int.MaxValue;
        _isRunning = true;
        _isDisposed = false;
        _taskSemaphore = new SemaphoreSlim(_maxConcurrentJobsCount, _maxConcurrentJobsCount);
        _repeatingWorkTimers = [];

        Task.Run(InitializeTaskQueue);
    }

    protected async Task EmitLog(LogMessage logMessage)
    {
        if (Log == null) return;

        await Log.Invoke(logMessage);
    }

    public void Dispose()
    {
        _isDisposed = true;
        _isRunning = false;

        foreach (var timer in _repeatingWorkTimers)
        {
            timer.Stop();
            timer.Dispose();
        }

        _repeatingWorkTimers.Clear();
        _channel.Dispose();
        GC.SuppressFinalize(this);
    }

    public void EnqueueJob(TJob job, Priority priority = Priority.Normal)
    {
        _channel.Write(job, priority);
    }

    private async Task InitializeTaskQueue()
    {
        while (_isDisposed == false)
        {
            while (_isRunning == false)
            {
                if (_isDisposed) return;

                await Task.Delay(1000);
            }

            TJob item;

            try
            {
                item = await _channel.ReadAsync();
            }
            catch (InvalidOperationException)
            {
                // All channels are closed.
                return;
            }

            await _taskSemaphore.WaitAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessWork(item);
                }
                finally
                {
                    _taskSemaphore.Release();
                }
            });
        }

        for (var i = 0; i < _maxConcurrentJobsCount; i++) await _taskSemaphore.WaitAsync();
    }

    private async Task ProcessWork(TJob item)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var jobHandler = scope.ServiceProvider.GetRequiredService<ICannoliProcessor<TJob>>();

            await jobHandler.HandleJobAsync(item);
        }
        catch (Exception ex)
        {
            await EmitLog(new LogMessage(
                LogSeverity.Error,
                GetType().Name,
                ex.Message,
                ex));
        }
    }

    public void ScheduleRepeatingJob(TimeSpan repeatEvery, TJob workItem, bool doWorkNow)
    {
        if (doWorkNow) EnqueueJob(workItem);

        var timer = new Timer(repeatEvery.TotalMilliseconds)
        {
            AutoReset = true
        };

        timer.Elapsed += (s, e) => { EnqueueJob(workItem); };

        timer.Start();

        _repeatingWorkTimers.Add(timer);
    }

    internal void StartTaskQueue()
    {
        _isRunning = true;
    }

    internal void StopTaskQueue()
    {
        _isRunning = false;
    }
}