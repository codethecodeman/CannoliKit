using Discord.WebSocket;

namespace DisCannoli.Workers.Jobs
{
    internal class DiscordMessageComponentJob
    {
        internal SocketMessageComponent MessageComponent { get; init; } = null!;
    }
}

