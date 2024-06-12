using CannoliKit.Models;

namespace CannoliKit.Modules
{
    /// <summary>
    /// Represents a base class for a Cannoli Module.
    /// </summary>
    public abstract class CannoliModuleBase
    {
        internal CannoliModuleBase() { }

        internal abstract Task<CannoliModuleFinalComponents> BuildComponents();
        internal abstract Task LoadModuleState(CannoliRoute route);
        internal abstract Task SaveModuleState();
    }
}
