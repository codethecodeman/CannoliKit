using CannoliKit.Interfaces;
using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
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

        public T CreateModule<T>(RouteConfiguration? routeConfiguration = null)
            where T : CannoliModuleBase
        {
            var constructor = typeof(T).GetConstructors().First();

            var parameters = constructor.GetParameters();

            var arguments = parameters.Select(p => p.ParameterType == typeof(RouteConfiguration)
                ? routeConfiguration
                : _serviceProvider.GetRequiredService(p.ParameterType)).ToArray();

            return (T)Activator.CreateInstance(typeof(T), arguments)!;
        }
    }
}
