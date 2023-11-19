using CannoliKit.Enums;
using CannoliKit.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Commands
{
    /// <inheritdoc/>
    public abstract class CannoliCommand<TContext> : CannoliCommandBase where TContext : DbContext, ICannoliDbContext
    {
        /// <inheritdoc/>
        public abstract override string Name { get; }

        /// <inheritdoc/>
        public abstract override DeferralType DeferralType { get; }

        /// <summary>
        /// The Cannoli client connected to this command.
        /// </summary>
        protected CannoliClient CannoliClient { get; private set; } = null!;

        /// <inheritdoc/>
        public abstract override ApplicationCommandProperties Build();

        internal override async Task Respond(ICannoliDbContext db, DiscordSocketClient discordClient, SocketCommandBase socketCommand)
        {
            await Respond((TContext)db, discordClient, socketCommand);
        }

        internal override void Setup(CannoliClient cannoliClient)
        {
            CannoliClient = cannoliClient;
        }

        /// <summary>
        /// Responds to an incoming command.
        /// </summary>
        /// <param name="db">The DbContext to use for data operations.</param>
        /// <param name="discordClient">The Cannoli client's connected Discord client.</param>
        /// <param name="command">The command object received by the Discord event.</param>
        /// <returns></returns>
        public abstract Task Respond(TContext db, DiscordSocketClient discordClient, SocketCommandBase command);
    }
}
