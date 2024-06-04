using CannoliKit.Attributes;
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
            services.AddSingleton<ICannoliClient, CannoliClient<TContext>>();

            services.AddCannoliProcessor<CannoliCleanupProcessor<TContext>, CannoliCleanupJob>(
                new CannoliJobQueueOptions
                {
                    MaxConcurrentJobs = 1
                });

            services.AddCannoliProcessor<CannoliCommandProcessor, CannoliCommandJob>();

            return services;
        }

        public static IServiceCollection AddCannoliCommand<T>(this IServiceCollection services)
            where T : class, ICannoliCommand
        {


            services.AddTransient<ICannoliCommand, T>();

            return services;
        }

        public static IServiceCollection AddCannoliProcessor<THandler, TJob>(this IServiceCollection services)
            where THandler : class, ICannoliProcessor<TJob>
        {

            var attribute = (CannoliProcessorAttribute?)
                GetType()
                    .GetCustomAttributes(typeof(CannoliProcessorAttribute), true)
                    .FirstOrDefault();

            services.AddSingleton<ICannoliJobQueue<TJob>, CannoliJobQueue<TJob>>();

            services.AddTransient<ICannoliProcessor<TJob>, THandler>();

            return services;
        }

        public static IServiceCollection AddCannoliServices2<TContext>(this IServiceCollection services)
            where TContext : DbContext, ICannoliDbContext
        {
            services.AddSingleton<CannoliClient<TContext>>();

            services.AddSingleton<ICannoliModuleFactory, CannoliModuleFactory>();

            var cannoliAssembly = Assembly.GetAssembly(typeof(ServiceCollectionExtensions))!;

            RegisterServicesFromAssembly(services, cannoliAssembly);

            var loadedAssemblies =
                AppDomain.CurrentDomain.GetAssemblies();

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

            var modules = FilterTypes(types, typeof(ICannoliModule));

            foreach (var module in modules)
            {
                services.AddTransient(typeof(ICannoliModule), module);
            }

            var commands = FilterTypes(types, typeof(ICannoliCommand));

            foreach (var command in commands)
            {
                var commandNameAttribute = command
                    .GetCustomAttributes(typeof(CannoliCommandAttribute), true)
                    .FirstOrDefault();

                if (commandNameAttribute == null)
                {
                    throw new InvalidOperationException(
                        $"Type {command} is missing {nameof(CannoliCommandAttribute)} and cannot be registered");
                }

                services.AddTransient(typeof(ICannoliCommand), command);
            }
        }

        private static IEnumerable<Type> FilterTypes(IEnumerable<Type> types, Type match)
        {
            return types
                .Where(x =>
                    match.IsAssignableFrom(x)
                    && x is { IsInterface: false, IsAbstract: false })
                .ToList();
        }
    }
}
