using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Workers.Channels;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Timer = System.Timers.Timer;

namespace CannoliKit.Workers;

public abstract class CannoliWorker<TContext, TJob> : ICannoliWorker
    where TContext : DbContext, ICannoliDbContext
{
    private readonly ICannoliWorkerChannel<TJob> _channel;
    private readonly ConcurrentBag<Timer> _repeatingWorkTimers;
    private readonly SemaphoreSlim _taskSemaphore;
    protected readonly int MaxConcurrentTaskCount;
    private bool _isRunning, _isDisposed;
    internal delegate Task LogEventHandler(LogMessage e);
    internal event LogEventHandler? Log;

    protected CannoliWorker(int maxConcurrentTaskCount)
    {
        _channel = new PriorityChannel<TJob>();

        MaxConcurrentTaskCount = maxConcurrentTaskCount;
        _isRunning = false;
        _isDisposed = false;
        _taskSemaphore = new SemaphoreSlim(MaxConcurrentTaskCount, MaxConcurrentTaskCount);
        _repeatingWorkTimers = [];

        Task.Run(InitializeTaskQueue);
    }

    internal CannoliWorker(int maxConcurrentTaskCount, ICannoliWorkerChannel<TJob> channel)
    {
        _channel = channel;

        MaxConcurrentTaskCount = maxConcurrentTaskCount;
        _isRunning = false;
        _isDisposed = false;
        _taskSemaphore = new SemaphoreSlim(MaxConcurrentTaskCount, MaxConcurrentTaskCount);
        _repeatingWorkTimers = [];

        Task.Run(InitializeTaskQueue);
    }
    protected async Task EmitLog(LogMessage logMessage)
    {
        if (Log == null) return;

        await Log.Invoke(logMessage);
    }

    protected CannoliClient<TContext> CannoliClient { get; private set; } = null!;

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

    internal void Setup(CannoliClient<TContext> cannoliClient)
    {
        CannoliClient = cannoliClient;
        StartTaskQueue();
    }

    public void EnqueueJob(TJob item, Priority priority = Priority.Normal)
    {
        _channel.Write(item, priority);
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

        for (var i = 0; i < MaxConcurrentTaskCount; i++) await _taskSemaphore.WaitAsync();
    }

    private async Task ProcessWork(TJob item)
    {
        try
        {
            await using var db = CannoliClient.GetDbContext();

            await DoWork(db, CannoliClient.DiscordClient, item);

            await db.SaveChangesAsync();
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

    public void StartTaskQueue()
    {
        _isRunning = true;
    }

    public void StopTaskQueue()
    {
        _isRunning = false;
    }

    protected abstract Task DoWork(TContext db, DiscordSocketClient discordClient, TJob item);
}