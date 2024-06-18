using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
using Discord.WebSocket;

namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a factory that creates an instance of a Cannoli Module, a type that implements <see cref="CannoliModule{TContext,TState}"/>.
    /// Automatically registered as a transient service at startup.
    /// </summary>
    public interface ICannoliModuleFactory
    {
        /// <summary>
        /// Create a new instance of a Cannoli Module, a type that implements <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <typeparam name="T">Type that implements <see cref="CannoliModule{TContext,TState}"/>.</typeparam>
        /// <param name="requestingUser">User that initiated the interaction.</param>
        /// <param name="routing">Route configuration, created using <see cref="RouteConfigurationBuilder"/>.</param>
        /// <returns>New instance of a Cannoli Module.</returns>
        T CreateModule<T>(
            SocketUser requestingUser,
            RouteConfiguration? routing = null)
            where T : CannoliModuleBase;

        /// <summary>
        /// Create a new instance of a Cannoli Module, a type that implements <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="type">Type that implements <see cref="CannoliModule{TContext,TState}"/>.</param>
        /// <param name="requestingUser">User that initiated the interaction.</param>
        /// <param name="routing">Route configuration, created using <see cref="RouteConfigurationBuilder"/>.</param>
        /// <returns>New instance of a Cannoli Module.</returns>
        CannoliModuleBase CreateModule(
            Type type,
            SocketUser requestingUser,
            RouteConfiguration? routing = null);
    }
}