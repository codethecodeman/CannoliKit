using CannoliKit.Commands;
using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Processors.Jobs;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CannoliKit
{
    public class CannoliClient : ICannoliClient
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CannoliRegistry _registry;
        private readonly IServiceProvider _serviceProvider;
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
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _discordClient = discordClient;
            _registry = _serviceProvider.GetRequiredService<CannoliRegistry>();
            _commandJobQueue = _serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliCommandJob>>();
            _moduleEventJobQueue = _serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliModuleEventJob>>();
            _cleanupJobQueue = _serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliCleanupJob>>();
        }

        internal IReadOnlyDictionary<string, Type> Commands => throw new NotImplementedException();

        public async Task SetupAsync()
        {
            await LoadCommands();
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
            if (_registry.Commands.IsEmpty) return;

            var remoteGlobalCommands = await _discordClient.GetGlobalApplicationCommandsAsync();

            foreach (var globalCommand in remoteGlobalCommands)
            {
                if (_registry.Commands.Keys.Any(c => c == globalCommand.Name))
                {
                    continue;
                }

                await globalCommand.DeleteAsync();

                _logger.LogInformation(
                    "Deleted global command {commandName}",
                    globalCommand.Name);
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var commands = scope.ServiceProvider.GetServices<ICannoliCommand>();

            var properties = new List<ApplicationCommandProperties>();

            foreach (var command in commands)
            {
                properties.Add(await command.BuildAsync());
            }

            await _discordClient.BulkOverwriteGlobalApplicationCommandsAsync(
                properties.ToArray());

            foreach (var commandName in _registry.Commands.Keys)
            {
                _logger.LogInformation(
                    "Registered global command {commandName}",
                    commandName);
            }
        }

        private async Task LoadCommands()
        {
            var commands = _serviceProvider.GetServices<ICannoliCommand>();

            foreach (var command in commands)
            {
                _registry.Commands[command.Name] = new CannoliCommandMeta
                {
                    Name = command.Name,
                    DeferralType = command.DeferralType,
                    ApplicationCommandProperties = await command.BuildAsync(),
                    Type = command.GetType()
                };
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
            _registry.Commands.TryGetValue(arg.CommandName, out var attributes);

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

            // return await RouteUtility.GetRoute(_db, customId);
            return new CannoliRoute();
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
