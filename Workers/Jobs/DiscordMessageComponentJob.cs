using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal class DiscordMessageComponentJob
    {
        internal SocketMessageComponent MessageComponent { get; init; } = null!;
    }
}

