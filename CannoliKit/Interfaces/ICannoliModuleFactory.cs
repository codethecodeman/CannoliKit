using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
using Discord.WebSocket;

namespace CannoliKit.Interfaces
{
    public interface ICannoliModuleFactory
    {
        T CreateModule<T>(
            SocketUser requestingUser,
            RouteConfiguration? routing = null)
            where T : CannoliModuleBase;

        CannoliModuleBase CreateModule(
            Type type,
            SocketUser requestingUser,
            RouteConfiguration? routing = null);
    }
}