using CannoliKit.Commands;
using CannoliKit.Interfaces;
using CannoliKit.Processors.Jobs;
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
        private readonly ILogger<CannoliCommandProcessor> _logger;

        public CannoliCommandProcessor(
            CannoliCommandRegistry commandRegistry,
            IServiceProvider serviceProvider,
            ILogger<CannoliCommandProcessor> logger)
        {
            _commandRegistry = commandRegistry;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task HandleJobAsync(CannoliCommandJob job)
        {
            _commandRegistry.Commands.TryGetValue(job.CommandName, out var attributes);

            if (attributes == null)
            {
                throw new InvalidOperationException(
                    $"Unable to find registered Cannoli command with name {job.CommandName}");
            }

            if (job.Command != null)
            {
                _logger.LogInformation(
                    "Received command {commandName} from user {username} ({userId})",
                    job.CommandName,
                    job.Command.User.Username,
                    job.Command.User.Id);
            }

            var command = (ICannoliCommand)_serviceProvider.GetRequiredService(attributes.Type);

            if (job.Command != null)
            {
                await command.RespondAsync(BuildContext(job));
            }

            if (job.Autocomplete != null
                && command is ICannoliAutocompleteCommand autocomplete)
            {
                await autocomplete.AutocompleteAsync(job.Autocomplete);
            }
        }

        private static CannoliCommandContext BuildContext(CannoliCommandJob job)
        {
            IApplicationCommandInteractionDataOption? subCommand = null;
            IApplicationCommandInteractionDataOption? subCommandGroup = null;

            if (job.Command is SocketSlashCommand slashCommand)
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
                Command = job.Command!,
                SubCommandGroup = subCommandGroup,
                SubCommand = subCommand,
            };
        }
    }
}
