using CannoliKit.Modules.Routing;
using Discord.WebSocket;

namespace CannoliKit.Modules
{
    public class CannoliModuleConfiguration
    {
        internal CannoliModuleConfiguration(
            SocketUser requestingUser,
            RouteConfiguration? routing = null)
        {
            RequestingUser = requestingUser;
            Routing = routing;
        }

        public SocketUser RequestingUser { get; init; }
        public RouteConfiguration? Routing { get; init; }
    }
}
