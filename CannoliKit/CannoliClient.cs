using CannoliKit.Attributes;
using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Processors.Jobs;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CannoliKit
{
    public class CannoliClient : ICannoliClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _discordClient;
        private readonly CannoliRegistry _registry;
        private readonly ICannoliJobQueue<CannoliCommandJob> _commandJobQueue;
        private readonly ICannoliJobQueue<CannoliModuleEventJob> _moduleEventJobQueue;
        private readonly ICannoliJobQueue<CannoliCleanupJob> _cleanupJobQueue;

        public CannoliClient(
            IServiceProvider serviceProvider,
            DiscordSocketClient discordClient)
        {
            _serviceProvider = serviceProvider;
            _discordClient = discordClient;
            _registry = _serviceProvider.GetRequiredService<CannoliRegistry>();
            _commandJobQueue = _serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliCommandJob>>();
            _moduleEventJobQueue = _serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliModuleEventJob>>();
            _cleanupJobQueue = _serviceProvider.GetRequiredService<ICannoliJobQueue<CannoliCleanupJob>>();
        }

        internal IReadOnlyDictionary<string, Type> Commands => throw new NotImplementedException();

        public void Setup()
        {
            LoadCommandAttributes();
            InitializeWorkers();
            SubscribeCommandEvents();
            SubscribeMessageComponentEvents();
            SubscribeModalEvents();
        }

        private void LoadCommandAttributes()
        {
            var types = _serviceProvider.GetServices<ICannoliCommand>();

            foreach (var type in types)
            {
                var commandAttribute = (CannoliCommandAttribute?)
                    type
                    .GetType()
                    .GetCustomAttributes(typeof(CannoliCommandAttribute), true)
                    .FirstOrDefault();

                if (commandAttribute == null)
                {
                    throw new InvalidOperationException(
                        $"Type {nameof(type)} is missing {nameof(CannoliCommandAttribute)} and cannot be loaded");
                }

                var commandName = commandAttribute.CommandName;
                _registry.Commands[commandName] = type.GetType();
                _registry.CommandAttributes[commandName] = commandAttribute;
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
            _registry.CommandAttributes.TryGetValue(arg.CommandName, out var attributes);

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
