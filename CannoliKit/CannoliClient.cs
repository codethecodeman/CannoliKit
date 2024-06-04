using CannoliKit.Attributes;
using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Processors.Core;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CannoliKit
{
    public class CannoliClient<TContext>
        where TContext : DbContext, ICannoliDbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _discordClient;
        private readonly TContext _db;
        private readonly Dictionary<string, Type> _commands = [];
        private readonly CannoliModuleEventProcessor<1>

        public CannoliClient(
            IServiceProvider serviceProvider,
            DiscordSocketClient discordClient,
            TContext dbContext)
        {
            _serviceProvider = serviceProvider;
            _discordClient = discordClient;
            _db = dbContext;
        }

        public void Setup()
        {
            RegisterCommandNames();
            InitializeWorkers();
            SubscribeCommandEvents();
            SubscribeMessageComponentEvents();
            SubscribeModalEvents();
        }

        private void RegisterCommandNames()
        {
            var commandTypes = _serviceProvider
                .GetServices(typeof(ICannoliCommand))
                .Select(x => x!.GetType())
                .ToList();

            foreach (var commandType in commandTypes)
            {
                var attribute = commandType
                    .GetCustomAttributes(typeof(CannoliCommandNameAttribute), true)
                    .First();

                var commandName = ((CannoliCommandNameAttribute)attribute).CommandName;

                _commands.Add(commandName, commandType);
            }
        }

        private void SubscribeCommandEvents()
        {
            _discordClient.SlashCommandExecuted += Enqueue;
            _discordClient.UserCommandExecuted += Enqueue;
            _discordClient.MessageCommandExecuted += Enqueue;
            return;

            async Task Enqueue(SocketCommandBase arg)
            {
                _ = Task.Run(async () =>
                {
                    await EnqueueCommandEvent(arg);
                });

                await Task.CompletedTask;
            }
        }

        private async Task EnqueueCommandEvent(SocketCommandBase arg)
        {
            if (_commands.ContainsKey(arg.CommandName) == false) return;

            var command = (ICannoliCommand)_serviceProvider.GetRequiredService(_commands[arg.CommandName]);

            if (command.DeferralType != DeferralType.None)
            {
                var isEphemeral = command.DeferralType == DeferralType.Ephemeral;
                await arg.DeferAsync(ephemeral: isEphemeral);
            }

            var jobQueue = _serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliCommandJob>>();

            jobQueue.EnqueueJob(
                new CannoliCommandJob()
                {
                    SocketCommand = arg,
                },
                (command.DeferralType == DeferralType.None) ? Priority.High : Priority.Normal);
        }

        private void SubscribeMessageComponentEvents()
        {
            _discordClient.ButtonExecuted += Enqueue;
            _discordClient.SelectMenuExecuted += Enqueue;
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
            _discordClient.ModalSubmitted += Enqueue;
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
            var worker = _serviceProvider.GetRequiredService<CannoliModuleEventProcessor<TContext>>();

            worker.EnqueueJob(
                cannoliModuleEventJob);

            await Task.CompletedTask;
        }

        private async Task<CannoliRoute?> GetRoute(string customId)
        {
            if (RouteUtility.IsValidRouteId(customId) == false) return null;

            return await RouteUtility.GetRoute(_db, customId);
        }

        private void InitializeWorkers()
        {
            var worker = _serviceProvider.GetRequiredService<CannoliCleanupProcessor<TContext>>();

            worker.ScheduleRepeatingJob(
                TimeSpan.FromMinutes(1),
                workItem: true,
                doWorkNow: true);
        }
    }
}
