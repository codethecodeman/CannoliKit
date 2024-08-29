using CannoliKit.Modules;

namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a Cannoli Module.
    /// </summary>
    public interface ICannoliModule
    {
        /// <summary>
        /// Build the module into components that can be applied to a Discord message.
        /// </summary>
        /// <returns>
        /// Cannoli module components.
        /// </returns>
        public Task<CannoliModuleComponents> BuildComponentsAsync();
    }
}
