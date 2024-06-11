using CannoliKit.Interfaces;
using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CannoliKit.Factories
{
    /// <summary>
    /// <inheritdoc cref="ICannoliModuleFactory"/>
    /// </summary>
    /// <typeparam name="TContext"><see cref="DbContext"/> that implements <see cref="ICannoliDbContext"/>.</typeparam>
    public sealed class CannoliModuleFactory<TContext> : ICannoliModuleFactory
        where TContext : DbContext, ICannoliDbContext
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="CannoliModuleFactory{TContext}"/>. This constructor is intended for use with Dependency Injection.
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

        private CannoliModuleBase CreateModuleFromType(
            Type type,
            SocketUser requestingUser,
            RouteConfiguration? routing = null)
        {
            var constructor = type.GetConstructors().First();

            var parameters = constructor.GetParameters();

            var configuration = new CannoliModuleFactoryConfiguration(requestingUser, routing);

            var arguments = parameters.Select(p => p.ParameterType == typeof(CannoliModuleFactoryConfiguration)
                ? configuration
                : _serviceProvider.GetRequiredService(p.ParameterType)).ToArray();

            return (CannoliModuleBase)Activator.CreateInstance(type, arguments)!;
        }
    }
}
