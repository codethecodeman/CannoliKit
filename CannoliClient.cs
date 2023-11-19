using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Registries;
using CannoliKit.Utilities;
using CannoliKit.Workers;
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
            async Task Enqueue(SocketCommandBase arg)
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
            }

            DiscordClient.SlashCommandExecuted += Enqueue;
            DiscordClient.UserCommandExecuted += Enqueue;
            DiscordClient.MessageCommandExecuted += Enqueue;
        }

        private void SubscribeMessageComponentEvents<TContext>()
            where TContext : DbContext, ICannoliDbContext
        {
            async Task Enqueue(SocketMessageComponent arg)
            {
                if (RouteUtility.IsValidRouteId(arg.Data.CustomId) == false) return;

                using var db = ((IDbContextFactory<TContext>)DbContextFactory)
                    .CreateDbContext();

                var route = await RouteUtility.GetRoute(db, RouteType.MessageComponent, arg.Data.CustomId);

                if (route == null || route.Priority == Priority.Normal)
                {
                    await arg.DeferAsync();
                }

                var worker = Workers.GetWorker<DiscordMessageComponentWorker<TContext>>()!;

                worker.EnqueueJob(
                    new DiscordMessageComponentJob()
                    {
                        MessageComponent = arg
                    },
                    route?.Priority ?? Priority.Normal);
            }

            DiscordClient.ButtonExecuted += Enqueue;
            DiscordClient.SelectMenuExecuted += Enqueue;
        }

        private void SubscribeModalEvents<TContext>() where TContext : DbContext, ICannoliDbContext
        {
            async Task Enqueue(SocketModal arg)
            {
                await arg.DeferAsync();

                var worker = Workers.GetWorker<DiscordModalWorker<TContext>>()!;

                worker.EnqueueJob(
                    new DiscordModalJob()
                    {
                        Modal = arg
                    },
                    priority: Priority.High);
            }

            DiscordClient.ModalSubmitted += Enqueue;
        }

        private void InitializeWorkers<TContext>()
            where TContext : DbContext, ICannoliDbContext
        {
            Workers.Add(new DiscordCommandWorker<TContext>(
                maxConcurrentTaskCount: int.MaxValue));

            Workers.Add(new DiscordMessageComponentWorker<TContext>(
                maxConcurrentTaskCount: int.MaxValue));

            Workers.Add(new DiscordModalWorker<TContext>(
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
