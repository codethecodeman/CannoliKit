using CannoliKit.Commands;
using CannoliKit.Interfaces;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CannoliKit
{
    internal class CannoliCommandRegistry
    {
        internal ConcurrentDictionary<string, CannoliCommandMeta> Commands { get; } = [];
        private readonly DiscordSocketClient _discordClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CannoliCommandRegistry> _logger;
        private bool _isLoaded;

        public CannoliCommandRegistry(
            DiscordSocketClient discordClient,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CannoliCommandRegistry> logger)
        {
            _discordClient = discordClient;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        internal async Task LoadCommands()
        {
            if (_isLoaded) return;

            using var scope = _serviceScopeFactory.CreateScope();
            var commands = scope.ServiceProvider.GetServices<ICannoliCommand>();

            foreach (var command in commands)
            {
                var meta = new CannoliCommandMeta
                {
                    Name = command.Name,
                    DeferralType = command.DeferralType,
                    ApplicationCommandProperties = await command.BuildAsync(),
                    Type = command.GetType()
                };

                meta.ApplicationCommandProperties.Name = command.Name;

                Commands[command.Name] = meta;
            }

            _isLoaded = true;
        }

        internal async Task RegisterCommandsWithDiscord()
        {
            if (Commands.IsEmpty) return;

            var remoteGlobalCommands = await _discordClient.GetGlobalApplicationCommandsAsync();

            foreach (var globalCommand in remoteGlobalCommands)
            {
                if (Commands.Keys.Any(c => c == globalCommand.Name))
                {
                    continue;
                }

                await globalCommand.DeleteAsync();

                _logger.LogInformation(
                    "Deleted global command {commandName}.",
                    globalCommand.Name);
            }

            await _discordClient.BulkOverwriteGlobalApplicationCommandsAsync(
                Commands.Values.Select(c => c.ApplicationCommandProperties).ToArray());

            foreach (var commandName in Commands.Keys)
            {
                _logger.LogInformation(
                    "Registered global command {commandName}.",
                    commandName);
            }
        }
    }
}
