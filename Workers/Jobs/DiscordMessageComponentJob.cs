using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal sealed class DiscordMessageComponentJob
    {
        internal SocketMessageComponent MessageComponent { get; init; } = null!;
    }
}

