using CannoliKit.Models;

namespace CannoliKit.Modules
{
    /// <summary>
    /// Represents a base class for a Cannoli Module.
    /// </summary>
    public abstract class CannoliModuleBase
    {
        internal CannoliModuleBase() { }

        /// <summary>
        /// Build the module into components that can be applied to a Discord message.
        /// </summary>
        /// <returns>
        /// Cannoli module components.
        /// </returns>
        public abstract Task<CannoliModuleComponents> BuildComponentsAsync();
        internal abstract Task LoadModuleState(CannoliRoute route);
        internal abstract Task SaveModuleState();
    }
}
