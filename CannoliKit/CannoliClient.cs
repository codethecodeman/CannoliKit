using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Processors.Jobs;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CannoliKit
{
    /// <summary>
    /// Client that initializes CannoliKit services and wires up events.
    /// </summary>
    /// <typeparam name="TContext">DbContext that implements <see cref="ICannoliDbContext"/>.</typeparam>
    public class CannoliClient<TContext> : ICannoliClient
        where TContext : DbContext, ICannoliDbContext
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CannoliCommandRegistry _commandRegistry;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ICannoliClient> _logger;
        private readonly ICannoliJobQueue<CannoliCommandJob> _commandJobQueue;
        private readonly ICannoliJobQueue<CannoliModuleEventJob> _moduleEventJobQueue;
        private readonly ICannoliJobQueue<CannoliCleanupJob> _cleanupJobQueue;

        public CannoliClient(
            DiscordSocketClient discordClient,
            IServiceProvider serviceProvider,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ICannoliClient> logger)
        {
            _discordClient = discordClient;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _commandRegistry = serviceProvider.GetRequiredService<CannoliCommandRegistry>();
            _commandJobQueue = serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliCommandJob>>();
            _moduleEventJobQueue = serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliModuleEventJob>>();
            _cleanupJobQueue = serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliCleanupJob>>();
        }

        /// <summary>
        /// Initializes CannoliKit services and wires up events.
        /// </summary>
        public async Task SetupAsync()
        {
            await _commandRegistry.LoadCommands();
            InitializeWorkers();
            SubscribeLoggedInEvent();
            SubscribeCommandEvents();
            SubscribeMessageComponentEvents();
            SubscribeModalEvents();
        }

        private void SubscribeLoggedInEvent()
        {
            _discordClient.Ready += RegisterCommands;
        }

        private async Task RegisterCommands()
        {
            if (_commandRegistry.Commands.IsEmpty) return;

            var remoteGlobalCommands = await _discordClient.GetGlobalApplicationCommandsAsync();

            foreach (var globalCommand in remoteGlobalCommands)
            {
                if (_commandRegistry.Commands.Keys.Any(c => c == globalCommand.Name))
                {
                    continue;
                }

                await globalCommand.DeleteAsync();

                _logger.LogInformation(
                    "Deleted global command {commandName}.",
                    globalCommand.Name);
            }

            await _discordClient.BulkOverwriteGlobalApplicationCommandsAsync(
                _commandRegistry.Commands.Values.Select(c => c.ApplicationCommandProperties).ToArray());

            foreach (var commandName in _commandRegistry.Commands.Keys)
            {
                _logger.LogInformation(
                    "Registered global command {commandName}.",
                    commandName);
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
            _commandRegistry.Commands.TryGetValue(arg.CommandName, out var attributes);

            if (attributes == null) return;

            if (attributes.DeferralType != DeferralType.None)
            {
                var isEphemeral = attributes.DeferralType == DeferralType.Ephemeral;
                await arg.DeferAsync(ephemeral: isEphemeral);
            }

            _commandJobQueue.EnqueueJob(
                new CannoliCommandJob()
                {
                    SocketCommand = arg,
                },
                (attributes.DeferralType == DeferralType.None) ? Priority.High : Priority.Normal);
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
            _moduleEventJobQueue.EnqueueJob(
                cannoliModuleEventJob);

            await Task.CompletedTask;
        }

        private async Task<CannoliRoute?> GetRoute(string customId)
        {
            if (RouteUtility.IsValidRouteId(customId) == false) return null;

            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();

            return await RouteUtility.GetRoute(db, customId);
        }

        private void InitializeWorkers()
        {
            _cleanupJobQueue.ScheduleRepeatingJob(
                repeatEvery: TimeSpan.FromMinutes(1),
                job: new CannoliCleanupJob(),
                doWorkNow: true);
        }
    }
}
