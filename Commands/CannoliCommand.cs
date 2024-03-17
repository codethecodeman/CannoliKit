using CannoliKit.Enums;
using CannoliKit.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Commands
{
    /// <summary>
    /// Represents a Discord command within Cannoli.
    /// </summary>
    public abstract class CannoliCommand<TContext>
    where TContext : DbContext, ICannoliDbContext
    {
        /// <summary>
        /// Gets the name of the command. This should be unique for each command.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the deferral type of the command, indicating how the command's execution is deferred when received.
        /// </summary>
        public abstract DeferralType DeferralType { get; }

        /// <summary>
        /// Handles the command's execution logic asynchronously.
        /// </summary>
        /// <param name="db">The database context for the command to interact with.</param>
        /// <param name="discordClient">The Discord client instance.</param>
        /// <param name="socketCommand">The socket command from the Discord event that triggered the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected abstract Task Respond(TContext db, DiscordSocketClient discordClient, CannoliCommandContext context);

        internal async Task RespondInternal(TContext db, DiscordSocketClient discordClient, SocketCommandBase socketCommand)
        {
            IApplicationCommandInteractionDataOption? subCommand = null;
            IApplicationCommandInteractionDataOption? subCommandGroup = null;

            if (socketCommand is SocketSlashCommand slashCommand)
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

            var context = new CannoliCommandContext
            {
                Command = socketCommand,
                SubCommandGroup = subCommandGroup,
                SubCommand = subCommand,
            };

            await Respond(db, discordClient, context);
        }

        /// <summary>
        /// Builds the command's properties for registration with Discord.
        /// </summary>
        /// <returns>The command properties to be used for registering the command with Discord.</returns>
        public abstract ApplicationCommandProperties Build();
    }
}
