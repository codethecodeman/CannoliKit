using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Workers.Channels;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Timer = System.Timers.Timer;

namespace CannoliKit.Workers
{
    public abstract class CannoliWorker<TContext, TJob> : CannoliWorkerBase, IDisposable where TContext : DbContext, ICannoliDbContext
    {
        protected readonly int MaxConcurrentTaskCount;
        public CannoliClient CannoliClient { get; private set; } = null!;
        private readonly PriorityChannel<TJob> _taskChannel;
        private readonly SemaphoreSlim _taskSemaphore;
        private readonly ConcurrentBag<Timer> _repeatingWorkTimers;
        private bool _isRunning, _isDisposed;

        protected CannoliWorker(int maxConcurrentTaskCount)
        {
            MaxConcurrentTaskCount = maxConcurrentTaskCount;

            _taskChannel = new PriorityChannel<TJob>();

            _isRunning = false;

            _isDisposed = false;

            _taskSemaphore = new SemaphoreSlim(MaxConcurrentTaskCount, MaxConcurrentTaskCount);

            _repeatingWorkTimers = new ConcurrentBag<Timer>();

            Task.Run(InitializeTaskQueue);
        }

        internal override void Setup(CannoliClient cannoliClient)
        {
            CannoliClient = cannoliClient;
            StartTaskQueue();
        }

        public void EnqueueJob(TJob item, Priority priority = Priority.Normal)
        {
            _taskChannel.Write(item, priority);
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
                    item = await _taskChannel.ReadAsync();
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

            for (var i = 0; i < MaxConcurrentTaskCount; i++)
            {
                await _taskSemaphore.WaitAsync();
            }
        }

        private async Task ProcessWork(TJob item)
        {
            try
            {
                var dbContextFactory = (IDbContextFactory<TContext>)CannoliClient.DbContextFactory;

                using var db = dbContextFactory.CreateDbContext();

                await DoWork(db, CannoliClient.DiscordClient, item);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await EmitLog(new LogMessage(
                    LogSeverity.Error,
                    GetType().Name,
                    ex.ToString(),
                    ex));
            }
        }

        public void ScheduleRepeatingJob(TimeSpan repeatEvery, TJob workItem, bool doWorkNow)
        {
            if (doWorkNow)
            {
                EnqueueJob(workItem);
            }

            var timer = new Timer(repeatEvery.TotalMilliseconds)
            {
                AutoReset = true
            };

            timer.Elapsed += (s, e) =>
            {
                EnqueueJob(workItem);
            };

            timer.Start();

            _repeatingWorkTimers.Add(timer);
        }

        public void StartTaskQueue() => _isRunning = true;

        public void StopTaskQueue() => _isRunning = false;

        protected abstract Task DoWork(TContext db, DiscordSocketClient discordClient, TJob item);

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
            _taskChannel.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

