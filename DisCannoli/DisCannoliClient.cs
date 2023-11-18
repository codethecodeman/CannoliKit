using DisCannoli.Enums;
using DisCannoli.Interfaces;
using DisCannoli.Utilities;
using DisCannoli.Workers;
using DisCannoli.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli
{
    public class DisCannoliClient
    {
        public delegate Task LogEventHandler(LogMessage e);
        public event LogEventHandler? Log;
        public DisCannoliWorkerRegistry Workers { get; }
        public DisCannoliCommandRegistry Commands { get; }
        public DiscordSocketClient DiscordClient { get; init; }
        internal object DbContextFactory { get; private set; } = null!;

        public DisCannoliClient(DiscordSocketClient discordClient)
        {
            DiscordClient = discordClient;
            Workers = new DisCannoliWorkerRegistry(this);
            Commands = new DisCannoliCommandRegistry(this);
        }

        public void Setup<TContext>(
            IDbContextFactory<TContext> dbContextFactory,
            int baseWorkerMaxConcurrentTaskCount = 16)
            where TContext : DbContext, IDisCannoliDbContext
        {
            DbContextFactory = dbContextFactory;
            InitializeWorkers<TContext>(baseWorkerMaxConcurrentTaskCount);
            SubscribeCommandEvents<TContext>();
            SubscribeMessageComponentEvents<TContext>();
            SubscribeModalEvents<TContext>();
        }

        private void SubscribeCommandEvents<TContext>()
            where TContext : DbContext, IDisCannoliDbContext
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
            where TContext : DbContext, IDisCannoliDbContext
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

        private void SubscribeModalEvents<TContext>() where TContext : DbContext, IDisCannoliDbContext
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

        private void InitializeWorkers<TContext>(int baseWorkerMaxConcurrentTaskCount)
            where TContext : DbContext, IDisCannoliDbContext
        {
            Workers.Add(new DiscordCommandWorker<TContext>(
                maxConcurrentTaskCount: baseWorkerMaxConcurrentTaskCount));

            Workers.Add(new DiscordMessageComponentWorker<TContext>(
                maxConcurrentTaskCount: baseWorkerMaxConcurrentTaskCount));

            Workers.Add(new DiscordModalWorker<TContext>(
                maxConcurrentTaskCount: baseWorkerMaxConcurrentTaskCount));

            var stateCleanupWorker = new DisCannoliCleanupWorker<TContext>(
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
