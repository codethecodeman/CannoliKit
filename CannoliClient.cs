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
    public class CannoliClient
    {
        public delegate Task LogEventHandler(LogMessage e);
        public event LogEventHandler? Log;
        public CannoliWorkerRegistry Workers { get; }
        public CannoliCommandRegistry Commands { get; }
        public DiscordSocketClient DiscordClient { get; init; }
        internal object DbContextFactory { get; private set; } = null!;

        /// <summary>
        /// Initializes a new Cannoli client with the provided Discord client.
        /// </summary>
        /// <param name="discordClient"></param>
        public CannoliClient(DiscordSocketClient discordClient)
        {
            DiscordClient = discordClient;
            Workers = new CannoliWorkerRegistry(this);
            Commands = new CannoliCommandRegistry(this);
        }

        /// <summary>
        /// Sets up the Cannoli client with the provided DbContextFactory.
        /// </summary>
        /// <typeparam name="TContext">A DbContext type which implements ICannoliDbContext.</typeparam>
        /// <param name="dbContextFactory">The DbContextFactory to use for the Cannoli client.</param>
        public void Setup<TContext>(
            IDbContextFactory<TContext> dbContextFactory)
            where TContext : DbContext, ICannoliDbContext
        {
            DbContextFactory = dbContextFactory;
            InitializeWorkers<TContext>();
            SubscribeCommandEvents<TContext>();
            SubscribeMessageComponentEvents<TContext>();
            SubscribeModalEvents<TContext>();
        }

        private void SubscribeCommandEvents<TContext>()
            where TContext : DbContext, ICannoliDbContext
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

        private void SubscribeMessageComponentEvents<TContext>()
            where TContext : DbContext, ICannoliDbContext
        {
            DiscordClient.ButtonExecuted += Enqueue;
            DiscordClient.SelectMenuExecuted += Enqueue;
            return;

            async Task Enqueue(SocketMessageComponent arg)
            {
                _ = Task.Run(async () =>
                {
                    await EnqueueModuleEvent<TContext>(arg);
                });

                await Task.CompletedTask;
            }
        }

        private void SubscribeModalEvents<TContext>() where TContext : DbContext, ICannoliDbContext
        {
            DiscordClient.ModalSubmitted += Enqueue;
            return;

            async Task Enqueue(SocketModal arg)
            {
                _ = Task.Run(async () =>
                {
                    await EnqueueModuleEvent<TContext>(arg);
                });

                await Task.CompletedTask;
            }
        }

        private async Task EnqueueModuleEvent<TContext>(SocketMessageComponent arg)
            where TContext : DbContext, ICannoliDbContext
        {
            var route = await GetRoute<TContext>(arg.Data.CustomId);

            if (route == null) return;

            if (route.Priority == Priority.Normal)
            {
                await arg.DeferAsync();
            }

            await EnqueueModuleEvent<TContext>(new CannoliModuleEventJob
            {
                Route = route,
                SocketMessageComponent = arg
            });
        }

        private async Task EnqueueModuleEvent<TContext>(SocketModal arg)
            where TContext : DbContext, ICannoliDbContext
        {
            var route = await GetRoute<TContext>(arg.Data.CustomId);

            if (route == null) return;

            await EnqueueModuleEvent<TContext>(new CannoliModuleEventJob
            {
                Route = route,
                SocketModal = arg
            });
        }

        private async Task EnqueueModuleEvent<TContext>(CannoliModuleEventJob cannoliModuleEventJob)
            where TContext : DbContext, ICannoliDbContext
        {
            var worker = Workers.GetWorker<CannoliModuleEventWorker<TContext>>()!;

            worker.EnqueueJob(
                cannoliModuleEventJob,
                cannoliModuleEventJob.Route.Priority);

            await Task.CompletedTask;
        }

        private async Task<CannoliRoute?> GetRoute<TContext>(string customId)
            where TContext : DbContext, ICannoliDbContext
        {
            if (RouteUtility.IsValidRouteId(customId) == false) return null;

            using var db = ((IDbContextFactory<TContext>)DbContextFactory)
                .CreateDbContext();

            return await RouteUtility.GetRoute(db, RouteType.MessageComponent, customId);
        }

        private void InitializeWorkers<TContext>()
            where TContext : DbContext, ICannoliDbContext
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
