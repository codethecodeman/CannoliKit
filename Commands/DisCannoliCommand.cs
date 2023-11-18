using DisCannoli.Enums;
using DisCannoli.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Commands
{
    public abstract class DisCannoliCommand<TContext> : DisCannoliCommandBase where TContext : DbContext, IDisCannoliDbContext
    {
        public abstract override string Name { get; }

        public abstract override DeferralType DeferralType { get; }

        protected DisCannoliClient DisCannoliClient { get; private set; } = null!;

        public abstract override ApplicationCommandProperties Build();

        public override async Task Respond(IDisCannoliDbContext db, DiscordSocketClient discordClient, SocketCommandBase socketCommand)
        {
            await Respond((TContext)db, discordClient, socketCommand);
        }

        internal override void Setup(DisCannoliClient disCannoliClient)
        {
            DisCannoliClient = disCannoliClient;
        }

        public abstract Task Respond(TContext db, DiscordSocketClient discordClient, SocketCommandBase command);

        protected string? GetOptionValue(IReadOnlyCollection<SocketSlashCommandDataOption> options, string key)
        {
            return options.First(o => o.Name == key).Value.ToString()?.Trim();
        }
    }
}
