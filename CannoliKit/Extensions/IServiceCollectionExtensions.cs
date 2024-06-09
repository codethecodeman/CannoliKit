using CannoliKit.Attributes;
using CannoliKit.Concurrency;
using CannoliKit.Factories;
using CannoliKit.Interfaces;
using CannoliKit.Processors;
using CannoliKit.Processors.Core;
using CannoliKit.Processors.Jobs;
using CannoliKit.Workers;
using CannoliKit.Workers.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CannoliKit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCannoliServices<TContext>(this IServiceCollection services)
            where TContext : DbContext, ICannoliDbContext
        {
            var cannoliAssembly = Assembly.GetAssembly(typeof(ServiceCollectionExtensions))!;

            // Discover services in loaded assemblies.

            var loadedAssemblies =
                AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in loadedAssemblies)
            {
                if (assembly.GetReferencedAssemblies().Any(a => a.Name == cannoliAssembly.GetName().Name))
                {
                    RegisterServicesFromAssembly(services, typeof(TContext), assembly);
                }
            }

            // Add core services.

            services.AddSingleton<ICannoliJobQueue<CannoliCleanupJob>, CannoliJobQueue<TContext, CannoliCleanupJob>>();
            services.AddTransient<ICannoliProcessor<CannoliCleanupJob>, CannoliCleanupProcessor<TContext>>();

            services.AddSingleton<ICannoliJobQueue<CannoliCommandJob>, CannoliJobQueue<TContext, CannoliCommandJob>>();
            services.AddTransient<ICannoliProcessor<CannoliCommandJob>, CannoliCommandProcessor>();

            services.AddSingleton<ICannoliJobQueue<CannoliModuleEventJob>, CannoliJobQueue<TContext, CannoliModuleEventJob>>();
            services.AddTransient<ICannoliProcessor<CannoliModuleEventJob>, CannoliModuleEventProcessor>();

            services.AddTransient<ICannoliModuleFactory, CannoliModuleFactory<TContext>>();

            services.AddSingleton<TurnManager>();
            services.AddSingleton<CannoliRegistry>();
            services.AddSingleton<ICannoliClient, CannoliClient<TContext>>();

            return services;
        }

        private static void RegisterServicesFromAssembly(IServiceCollection services, Type dbContextType, Assembly assembly)
        {
            var types = assembly.GetTypes();

            RegisterProcessors(services, dbContextType, types);
            RegisterCommands(services, types);
        }

        private static void RegisterProcessors(IServiceCollection services, Type dbContextType, IEnumerable<Type> types)
        {
            var processors = FilterTypes(types, typeof(ICannoliProcessor<>));

            foreach (var processor in processors)
            {
                var attribute = (CannoliProcessorAttribute?)
                    processor
                        .GetCustomAttributes(typeof(CannoliProcessorAttribute), true)
                        .FirstOrDefault();

                var processorInterfaceType = processor.GetInterface(typeof(ICannoliProcessor<>).Name)!;
                var jobType = processorInterfaceType.GetGenericArguments()[0];
                var jobQueueInterfaceType = typeof(ICannoliJobQueue<>).MakeGenericType(jobType);
                var jobQueueType = typeof(CannoliJobQueue<,>).MakeGenericType(dbContextType, jobType);

                services.AddSingleton(jobQueueInterfaceType, sp =>
                {
                    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                    var options = new CannoliJobQueueOptions
                    {
                        MaxConcurrentJobs = attribute?.MaxConcurrentJobs ?? int.MaxValue
                    };
                    return Activator.CreateInstance(jobQueueType, sp, scopeFactory, options)!;
                });

                services.AddTransient(processorInterfaceType, processor);
            }
        }

        private static void RegisterCommands(IServiceCollection services, IEnumerable<Type> types)
        {
            var commands = FilterTypes(types, typeof(ICannoliCommand));

            foreach (var command in commands)
            {
                services.AddTransient(typeof(ICannoliCommand), command);
                services.AddTransient(command);
            }
        }

        private static List<Type> FilterTypes(IEnumerable<Type> types, Type match)
        {
            return types
                .Where(x =>
                    x.GetInterfaces().Any(i =>
                        (i.IsGenericType && i.GetGenericTypeDefinition() == match) || i == match)
                    && x is { IsInterface: false, IsAbstract: false })
                .ToList();
        }
    }
}
