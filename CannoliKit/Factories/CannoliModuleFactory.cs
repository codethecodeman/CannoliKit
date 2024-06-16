using CannoliKit.Interfaces;
using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CannoliKit.Factories
{
    /// <summary>
    /// <inheritdoc cref="ICannoliModuleFactory"/>
    /// </summary>
    public sealed class CannoliModuleFactory : ICannoliModuleFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="CannoliModuleFactory"/>. This constructor is intended for use with Dependency Injection.
        /// </summary>
        public CannoliModuleFactory(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// <inheritdoc cref="ICannoliModuleFactory"/>
        /// </summary>
        public T CreateModule<T>(
            SocketUser requestingUser,
            RouteConfiguration? routing = null)
            where T : CannoliModuleBase
        {
            return (T)CreateModuleFromType(typeof(T), requestingUser, routing);
        }

        /// <summary>
        /// <inheritdoc cref="ICannoliModuleFactory"/>
        /// </summary>
        public CannoliModuleBase CreateModule(
            Type type,
            SocketUser requestingUser,
            RouteConfiguration? routing = null)
        {
            return CreateModuleFromType(type, requestingUser, routing);
        }

        internal CannoliModuleBase CreateModule(
            Type type,
            SocketUser requestingUser,
            SocketInteraction interaction,
            RouteConfiguration? routing = null
           )
        {
            return CreateModuleFromType(type, requestingUser, routing, interaction);
        }

        private CannoliModuleBase CreateModuleFromType(
            Type type,
            SocketUser requestingUser,
            RouteConfiguration? routing = null,
            SocketInteraction? interaction = null
            )
        {
            var constructor = type.GetConstructors().First();

            var parameters = constructor.GetParameters();

            var configuration = new CannoliModuleFactoryConfiguration(requestingUser, routing, interaction);

            var arguments = parameters.Select(p => p.ParameterType == typeof(CannoliModuleFactoryConfiguration)
                ? configuration
                : _serviceProvider.GetRequiredService(p.ParameterType)).ToArray();

            return (CannoliModuleBase)Activator.CreateInstance(type, arguments)!;
        }
    }
}
