using CannoliKit.Models;
using Discord.WebSocket;

namespace CannoliKit.Processors.Jobs
{
    internal sealed class CannoliModuleEventJob
    {

        internal CannoliRoute Route { get; init; } = null!;

        internal SocketMessageComponent? SocketMessageComponent { get; init; }

        internal SocketModal? SocketModal { get; init; }
    }
}
