using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    public sealed class DiscordModalJob
    {
        internal DiscordModalJob() { }
        internal SocketModal Modal { get; set; } = null!;
    }
}
