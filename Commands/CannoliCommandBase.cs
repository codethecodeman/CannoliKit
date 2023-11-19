using CannoliKit.Enums;
using CannoliKit.Interfaces;
using Discord;
using Discord.WebSocket;

namespace CannoliKit.Commands
{
    /// <summary>
    /// Represents a Discord command within Cannoli.
    /// </summary>
    public abstract class CannoliCommandBase
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
        internal abstract Task Respond(ICannoliDbContext db, DiscordSocketClient discordClient, SocketCommandBase socketCommand);

        internal abstract void Setup(CannoliClient cannoliClient);

        /// <summary>
        /// Builds the command's properties for registration with Discord.
        /// </summary>
        /// <returns>The command properties to be used for registering the command with Discord.</returns>
        public abstract ApplicationCommandProperties Build();
    }
}
