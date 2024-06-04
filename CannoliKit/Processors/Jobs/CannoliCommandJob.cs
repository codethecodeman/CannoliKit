using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal sealed class CannoliCommandJob
    {
        internal SocketCommandBase SocketCommand { get; init; } = null!;
    }
}
