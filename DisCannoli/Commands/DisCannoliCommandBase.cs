using DisCannoli.Enums;
using DisCannoli.Interfaces;
using Discord;
using Discord.WebSocket;

namespace DisCannoli.Commands
{
    public abstract class DisCannoliCommandBase
    {
        public abstract string Name { get; }
        public abstract DeferralType DeferralType { get; }

        public abstract Task Respond(IDisCannoliDbContext db, DiscordSocketClient discordClient, SocketCommandBase socketCommand);

        internal abstract void Setup(DisCannoliClient disCannoliClient);

        public abstract ApplicationCommandProperties Build();
    }
}
