using CannoliKit.Models;

namespace CannoliKit.Interfaces
{
    internal interface ICannoliModuleRouter
    {
        Task RouteToModuleCallback(CannoliRoute route, object parameter);
    }
}
