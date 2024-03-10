using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Registries;
using CannoliKit.Utilities;
using CannoliKit.Workers.Core;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit
{
    public class CannoliClient<TContext>
    where TContext : DbContext, ICannoliDbContext
    {
        public delegate Task LogEventHandler(LogMessage e);
        public event LogEventHandler? Log;
        public CannoliWorkerRegistry<TContext> Workers { get; }
        public CannoliCommandRegistry<TContext> Commands { get; }
        public DiscordSocketClient DiscordClient { get; private set; } = null!;
        internal IDbContextFactory<TContext> DbContextFactory { get; private set; } = null!;

        public CannoliClient()
        {
            Workers = new CannoliWorkerRegistry<TContext>(this);
            Commands = new CannoliCommandRegistry<TContext>(this);
        }

        public void Setup(
            DiscordSocketClient discordClient,
            IDbContextFactory<TContext> dbContextFactory)
        {
            DiscordClient = discordClient;
            DbContextFactory = dbContextFactory;
            InitializeWorkers();
            SubscribeCommandEvents();
            SubscribeMessageComponentEvents();
            SubscribeModalEvents();
        }

        public TContext GetDbContext()
        {
            return DbContextFactory.CreateDbContext();
        }

        private void SubscribeCommandEvents()
        {
            DiscordClient.SlashCommandExecuted += Enqueue;
            DiscordClient.UserCommandExecuted += Enqueue;
            DiscordClient.MessageCommandExecuted += Enqueue;
            return;

            async Task Enqueue(SocketCommandBase arg)
            {
                _ = Task.Run(async () =>
                {
                    var command = Commands.GetCommand(arg.CommandName);

                    if (command == null) return;

                    if (command.DeferralType != DeferralType.None)
                    {
                        var isEphemeral = command.DeferralType == DeferralType.Ephemeral;
                        await arg.DeferAsync(ephemeral: isEphemeral);
                    }

                    var worker = Workers.GetWorker<DiscordCommandWorker<TContext>>()!;

                    worker.EnqueueJob(
                        new DiscordCommandJob()
                        {
                            SocketCommand = arg,
                        },
                        (command.DeferralType == DeferralType.None) ? Priority.High : Priority.Normal);
                });

                await Task.CompletedTask;
            }
        }

        private void SubscribeMessageComponentEvents()
        {
            DiscordClient.ButtonExecuted += Enqueue;
            DiscordClient.SelectMenuExecuted += Enqueue;
            return;

            async Task Enqueue(SocketMessageComponent arg)
            {
                _ = Task.Run(async () =>
                {
                    await EnqueueModuleEvent(arg);
                });

                await Task.CompletedTask;
            }
        }

        private void SubscribeModalEvents()
        {
            DiscordClient.ModalSubmitted += Enqueue;
            return;

            async Task Enqueue(SocketModal arg)
            {
                _ = Task.Run(async () =>
                {
                    await EnqueueModuleEvent(arg);
                });

                await Task.CompletedTask;
            }
        }

        private async Task EnqueueModuleEvent(SocketMessageComponent arg)
        {
            var route = await GetRoute(arg.Data.CustomId);

            if (route == null) return;

            if (route.IsDeferred)
            {
                await arg.DeferAsync();
            }

            await EnqueueModuleEvent(new CannoliModuleEventJob
            {
                Route = route,
                SocketMessageComponent = arg
            });
        }

        private async Task EnqueueModuleEvent(SocketModal arg)
        {
            var route = await GetRoute(arg.Data.CustomId);

            if (route == null) return;

            await arg.DeferAsync();

            await EnqueueModuleEvent(new CannoliModuleEventJob
            {
                Route = route,
                SocketModal = arg
            });
        }

        private async Task EnqueueModuleEvent(CannoliModuleEventJob cannoliModuleEventJob)
        {
            var worker = Workers.GetWorker<CannoliModuleEventWorker<TContext>>()!;

            worker.EnqueueJob(
                cannoliModuleEventJob);

            await Task.CompletedTask;
        }

        private async Task<CannoliRoute?> GetRoute(string customId)
        {
            if (RouteUtility.IsValidRouteId(customId) == false) return null;

            await using var db = GetDbContext();

            return await RouteUtility.GetRoute(db, customId);
        }

        private void InitializeWorkers()
        {
            Workers.Add(new DiscordCommandWorker<TContext>(
                maxConcurrentTaskCount: int.MaxValue));

            Workers.Add(new CannoliModuleEventWorker<TContext>(
                maxConcurrentTaskCount: int.MaxValue));

            var stateCleanupWorker = new CannoliCleanupWorker<TContext>(
                maxConcurrentTaskCount: 1);

            stateCleanupWorker.ScheduleRepeatingJob(
                TimeSpan.FromMinutes(1),
                workItem: true,
                doWorkNow: true);

            Workers.Add(stateCleanupWorker);
        }

        internal async Task EmitLog(LogMessage logMessage)
        {
            if (Log == null) return;

            await Log.Invoke(logMessage);
        }
    }
}
