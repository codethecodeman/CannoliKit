using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace CannoliKit.Processors.Core
{
    internal class CannoliModuleEventProcessor<TContext> : CannoliJobQueue<>
        where TContext : DbContext, ICannoliDbContext
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _turns = new();

        public CannoliModuleEventProcessor(int maxConcurrentTaskCount) : base(maxConcurrentTaskCount)
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
                _ = ProcessRoute(item.Route, paramToPass);

                return;
            }

            _ = EnqueueItem(item, paramToPass);

            await Task.CompletedTask;
        }

        private async Task ProcessRoute(CannoliRoute route, object paramToPass)
        {
            await using var db = CannoliClient.GetDbContext();

            await RouteUtility.RouteToModuleCallback(
                db,
                CannoliClient.DiscordClient,
                route,
                paramToPass);

            await db.SaveChangesAsync();
        }

        private async Task EnqueueItem(CannoliModuleEventJob item, object paramToPass)
        {
            var thisTurn = new TaskCompletionSource<bool>();

            var previousTurn = GetTurnToAwait(item.Route.StateId, thisTurn);

            if (previousTurn != null)
            {
                await previousTurn.Task;
            }

            try
            {
                await ProcessRoute(item.Route, paramToPass);
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
                thisTurn.SetResult(true);
                CleanupTurns();
            }
        }

        private TaskCompletionSource<bool>? GetTurnToAwait(string stateId, TaskCompletionSource<bool> nextTurn)
        {
            lock (_turns)
            {
                TaskCompletionSource<bool>? currentTurn = null;

                if (_turns.TryGetValue(stateId, out var tcs))
                {
                    currentTurn = tcs;
                }

                _turns[stateId] = nextTurn;

                return currentTurn;
            }
        }

        private void CleanupTurns()
        {
            lock (_turns)
            {
                var completedEntries = _turns
                    .Where(x => x.Value.Task.IsCompleted)
                    .ToList();

                foreach (var completedEntry in completedEntries)
                {
                    _turns.TryRemove(completedEntry.Key, out _);
                }
            }
        }
    }
}
