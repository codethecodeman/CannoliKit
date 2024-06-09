using CannoliKit.Modules;
using CannoliKit.Modules.Routing;

namespace CannoliKit.Interfaces
{
    public interface ICannoliModuleFactory
    {
        T CreateModule<T>(RouteConfiguration? routeConfiguration = null)
            where T : CannoliModuleBase;
    }
}
