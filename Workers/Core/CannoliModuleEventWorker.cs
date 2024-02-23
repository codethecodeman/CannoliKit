using CannoliKit.Interfaces;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace CannoliKit.Workers.Core
{
    internal class CannoliModuleEventWorker<TContext> : CannoliWorker<TContext, CannoliModuleEventJob> where TContext : DbContext, ICannoliDbContext
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _groupSemaphores = new();
        private readonly ConcurrentDictionary<string, Queue<TaskCompletionSource<bool>>> _groupQueues = new();

        public CannoliModuleEventWorker(int maxConcurrentTaskCount) : base(maxConcurrentTaskCount)
        {
        }

        protected override async Task DoWork(TContext db, DiscordSocketClient discordClient, CannoliModuleEventJob item)
        {
            object paramToPass = null!;

            if (item.SocketMessageComponent != null)
            {
                paramToPass = item.SocketMessageComponent;
            }
            else if (item.SocketModal != null)
            {
                paramToPass = item.SocketModal;
            }

            if (item.Route.IsSynchronous == false)
            {
                _ = RouteUtility.RouteToModuleCallback(
                    db,
                    discordClient,
                    item.Route,
                    paramToPass);

                return;
            }

            _ = EnqueueItem(db, discordClient, item, paramToPass);

            await Task.CompletedTask;
        }

        private async Task EnqueueItem(TContext db, DiscordSocketClient discordClient, CannoliModuleEventJob item, object parameter)
        {
            var queue = _groupQueues.GetOrAdd(item.Route.StateId, _ => new Queue<TaskCompletionSource<bool>>());

            var myTurn = new TaskCompletionSource<bool>();
            TaskCompletionSource<bool>? previousTurn = null;

            lock (queue)
            {
                if (queue.Count > 0)
                {
                    previousTurn = queue.Last()!; // Get the last TCS in the queue
                }

                queue.Enqueue(myTurn);
            }

            if (previousTurn != null)
            {
                await previousTurn.Task;
            }

            try
            {
                _ = RouteUtility.RouteToModuleCallback(
                    db,
                    discordClient,
                    item.Route,
                    parameter);
            }
            catch (Exception ex)
            {
                await EmitLog(new LogMessage(
                    LogSeverity.Error,
                    GetType().Name,
                    ex.Message,
                    ex));
            }
            finally
            {
                lock (queue)
                {
                    queue.Dequeue();

                    if (queue.TryPeek(out var nextTurn))
                    {
                        nextTurn.SetResult(true);
                    }
                    else
                    {
                        if (queue.Count == 0)
                        {
                            _groupQueues.TryGetValue(item.Route.StateId, out var currentQueue);

                            if (currentQueue == queue)
                            {
                                _groupQueues.TryRemove(
                                    item.Route.StateId,
                                    out _);
                            }
                        }
                    }
                }
            }
        }
    }
}
