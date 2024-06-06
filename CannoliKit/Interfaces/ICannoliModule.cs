using CannoliKit.Models;

namespace CannoliKit.Interfaces
{
    internal interface ICannoliModule
    {
        Task LoadModuleState(CannoliRoute route);
        Task SaveModuleState();
    }
}
