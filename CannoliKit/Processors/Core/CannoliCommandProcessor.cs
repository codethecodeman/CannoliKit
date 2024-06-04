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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        internal CannoliCommandProcessor(
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task HandleJobAsync(CannoliCommandJob job)
        {
            var commandName = job.SocketCommand.CommandName;

            CannoliRegistry.Commands.TryGetValue(commandName, out var commandType);

            if (commandType == null)
            {
                throw new InvalidOperationException(
                    $"Unable to find registered Cannoli command with name \"{commandName}\"");
            }

            _logger.LogInformation(
                "Received command \"{commandName}\" from user {userId} ({username})",
                commandName,
                job.SocketCommand.User.Id,
                job.SocketCommand.User.Username);

            var command = (ICannoliCommand)_serviceProvider.GetRequiredService(commandType);

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
                        .First(x => x.Type == ApplicationCommandOptionType.SubCommand);
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