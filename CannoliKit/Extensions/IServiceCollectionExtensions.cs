using CannoliKit.Factories;
using CannoliKit.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CannoliKit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Type[] SingletonCannoliInterfaces =
        [
            typeof(ICannoliWorker),
        ];

        private static readonly Type[] TransientCannoliInterfaces =
        [
            typeof(ICannoliModule),
            typeof(ICannoliCommand)
        ];

        public static IServiceCollection AddCannoliServices<TContext>(this IServiceCollection services)
            where TContext : DbContext, ICannoliDbContext
        {
            services.AddSingleton<CannoliClient<TContext>>();

            services.AddSingleton<ICannoliModuleFactory, CannoliModuleFactory>();

            var cannoliAssembly = Assembly.GetAssembly(typeof(ServiceCollectionExtensions))!;

            RegisterServicesFromAssembly(services, cannoliAssembly);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in loadedAssemblies)
            {
                if (assembly.GetReferencedAssemblies().Any(a => a.Name == cannoliAssembly.GetName().Name))
                {
                    RegisterServicesFromAssembly(services, assembly);
                }
            }

            return services;
        }

        private static void RegisterServicesFromAssembly(IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                foreach (var cannoliInterface in SingletonCannoliInterfaces)
                {
                    if (cannoliInterface.IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                    {
                        services.AddSingleton(cannoliInterface, type);
                    }
                }

                foreach (var cannoliInterface in TransientCannoliInterfaces)
                {
                    if (cannoliInterface.IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                    {
                        services.AddTransient(cannoliInterface, type);
                    }
                }
            }
        }

    }
}
