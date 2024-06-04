using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal sealed class DiscordModalJob
    {
        public SocketModal Modal { get; set; } = null!;
    }
}
