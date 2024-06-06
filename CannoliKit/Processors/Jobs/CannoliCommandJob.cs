using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    public sealed class CannoliCommandJob
    {
        internal CannoliCommandJob() { }
        internal SocketCommandBase SocketCommand { get; init; } = null!;
    }
}
