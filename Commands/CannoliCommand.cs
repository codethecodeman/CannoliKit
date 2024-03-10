using CannoliKit.Enums;
using CannoliKit.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Commands
{
    /// <inheritdoc/>
    public abstract class CannoliCommand<TContext> : CannoliCommandBase<TContext>
        where TContext : DbContext, ICannoliDbContext
    {
        /// <inheritdoc/>
        public abstract override string Name { get; }

        /// <inheritdoc/>
        public abstract override DeferralType DeferralType { get; }

        /// <inheritdoc/>
        public abstract override ApplicationCommandProperties Build();

        /// <summary>
        /// Responds to an incoming command.
        /// </summary>
        /// <param name="db">The DbContext to use for data operations.</param>
        /// <param name="discordClient">The Cannoli client's connected Discord client.</param>
        /// <param name="command">The command object received by the Discord event.</param>
        /// <returns></returns>
        protected abstract override Task Respond(TContext db, DiscordSocketClient discordClient, SocketCommandBase command);
    }
}
