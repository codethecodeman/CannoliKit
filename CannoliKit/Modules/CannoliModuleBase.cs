using CannoliKit.Models;

namespace CannoliKit.Modules
{
    public abstract class CannoliModuleBase
    {
        internal CannoliModuleBase() { }

        internal abstract Task<CannoliModuleComponents> BuildComponents();
        internal abstract Task LoadModuleState(CannoliRoute route);
        internal abstract Task SaveModuleState();
    }
}
