using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal class DiscordCommandJob
    {
        internal SocketCommandBase SocketCommand { get; init; } = null!;
    }
}
