namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a client that initializes Cannoli services and wires up events. Automatically registered as a singleton service at startup.
    /// </summary>
    public interface ICannoliClient
    {
        /// <summary>
        /// Initializes Cannoli services and wires up events.
        /// </summary>
        Task SetupAsync();
    }
}
