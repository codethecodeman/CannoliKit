﻿using CannoliKit.Commands;
using CannoliKit.Interfaces;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CannoliKit.Processors.Core
{
    internal sealed class CannoliCommandProcessor : ICannoliProcessor<CannoliCommandJob>
    {
        private readonly CannoliCommandRegistry _commandRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CannoliCommandJob> _logger;

        public CannoliCommandProcessor(
            CannoliCommandRegistry commandRegistry,
            IServiceProvider serviceProvider,
            ILogger<CannoliCommandJob> logger)
        {
            _commandRegistry = commandRegistry;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task HandleJobAsync(CannoliCommandJob job)
        {
            var commandName = job.SocketCommand.CommandName;

            _commandRegistry.Commands.TryGetValue(commandName, out var attributes);

            if (attributes == null)
            {
                throw new InvalidOperationException(
                    $"Unable to find registered Cannoli command with name {commandName}");
            }

            _logger.LogInformation(
                "Received command {commandName} from user {username} ({userId})",
                commandName,
                job.SocketCommand.User.Username,
                job.SocketCommand.User.Id);

            var command = (ICannoliCommand)_serviceProvider.GetRequiredService(attributes.Type);

            await command.RespondAsync(BuildContext(job));
        }

        private static CannoliCommandContext BuildContext(CannoliCommandJob job)
        {
            IApplicationCommandInteractionDataOption? subCommand = null;
            IApplicationCommandInteractionDataOption? subCommandGroup = null;

            if (job.SocketCommand is SocketSlashCommand slashCommand)
            {
                subCommandGroup =
                    slashCommand.Data.Options.FirstOrDefault(
                        x => x.Type == ApplicationCommandOptionType.SubCommandGroup);

                if (subCommandGroup != null)
                {
                    subCommand = subCommandGroup.Options
                        .First(x => x.Type == ApplicationCommandOptionType.SubCommand);
                }
                else
                {
                    subCommand = slashCommand.Data.Options
                        .FirstOrDefault(x => x.Type == ApplicationCommandOptionType.SubCommand);
                }
            }

            return new CannoliCommandContext
            {
                Command = job.SocketCommand,
                SubCommandGroup = subCommandGroup,
                SubCommand = subCommand,
            };
        }
    }
}
