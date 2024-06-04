using CannoliKit.Models;
using Discord.WebSocket;

namespace CannoliKit.Workers.Jobs
{
    internal class CannoliModuleEventJob
    {
        public CannoliRoute Route { get; init; } = null!;

        public SocketMessageComponent? SocketMessageComponent { get; init; }

        public SocketModal? SocketModal { get; init; }
    }
}
