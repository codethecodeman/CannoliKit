using CannoliKit.Interfaces;
using CannoliKit.Models;

namespace CannoliKit.Modules
{
    /// <summary>
    /// Represents a base class for a Cannoli Module.
    /// </summary>
    public abstract class CannoliModuleBase : ICannoliModule
    {
        internal CannoliModuleBase() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract Task<CannoliModuleComponents> BuildComponentsAsync();
        internal abstract Task LoadModuleState(CannoliRoute route);
        internal abstract Task SaveModuleState();
    }
}
