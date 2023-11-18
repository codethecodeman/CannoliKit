using Discord.WebSocket;

namespace DisCannoli.Workers.Jobs
{
    internal class DiscordModalJob
    {
        public SocketModal Modal { get; set; } = null!;
    }
}
