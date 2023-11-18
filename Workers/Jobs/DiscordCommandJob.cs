using Discord.WebSocket;

namespace DisCannoli.Workers.Jobs
{
    internal class DiscordCommandJob
    {
        internal SocketCommandBase SocketCommand { get; init; } = null!;
    }
}
