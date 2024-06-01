using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal sealed class DiscordCommandJob
    {
        internal SocketCommandBase SocketCommand { get; init; } = null!;
    }
}
