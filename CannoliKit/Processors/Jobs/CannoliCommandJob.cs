using Discord.WebSocket;

namespace CannoliKit.Processors.Jobs
{
    internal sealed class CannoliCommandJob
    {
        internal SocketCommandBase SocketCommand { get; init; } = null!;
    }
}
