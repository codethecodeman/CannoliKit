using Discord;
using Discord.WebSocket;

namespace CannoliKit.Commands
{
    /// <summary>
    /// Encapsulates context for an incoming Discord command interaction.
    /// </summary>
    public sealed class CannoliCommandContext
    {
        /// <summary>
        /// Command that initiated this interaction.
        /// </summary>
        public SocketCommandBase Command { get; init; } = null!;

        /// <summary>
        /// If command contains subcommand groups, the option group representing the subcommand group. Null if not applicable.
        /// </summary>
        public IApplicationCommandInteractionDataOption? SubCommandGroupOption { get; init; }

        /// <summary>
        /// If command contains subcommands, the option group representing the subcommand. Null if not applicable.
        /// </summary>
        public IApplicationCommandInteractionDataOption? SubCommandOption { get; init; }
    }
}
