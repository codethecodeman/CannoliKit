using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    public sealed class DiscordMessageComponentJob
    {
        internal DiscordMessageComponentJob() { }

        internal SocketMessageComponent MessageComponent { get; init; } = null!;
    }
}

