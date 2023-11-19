using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal class DiscordModalJob
    {
        public SocketModal Modal { get; set; } = null!;
    }
}
