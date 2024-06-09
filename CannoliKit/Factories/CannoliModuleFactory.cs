using CannoliKit.Interfaces;
using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CannoliKit.Factories
{
    public sealed class CannoliModuleFactory<TContext> : ICannoliModuleFactory
        where TContext : DbContext, ICannoliDbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public CannoliModuleFactory(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T CreateModule<T>(
            SocketUser requestingUser,
            RouteConfiguration? routing = null)
            where T : CannoliModuleBase
        {
            return (T)CreateModuleFromType(typeof(T), requestingUser, routing);
        }

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

            var configuration = new CannoliModuleConfiguration(requestingUser, routing);

            var arguments = parameters.Select(p => p.ParameterType == typeof(CannoliModuleConfiguration)
                ? configuration
                : _serviceProvider.GetRequiredService(p.ParameterType)).ToArray();

            return (CannoliModuleBase)Activator.CreateInstance(type, arguments)!;
        }
    }
}
