using CannoliKit.Models;
using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    public sealed class CannoliModuleEventJob
    {
        internal CannoliModuleEventJob() { }

        internal CannoliRoute Route { get; init; } = null!;

        internal SocketMessageComponent? SocketMessageComponent { get; init; }

        internal SocketModal? SocketModal { get; init; }
    }
}
