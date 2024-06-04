using CannoliKit.Attributes;
using CannoliKit.Factories;
using CannoliKit.Interfaces;
using CannoliKit.Processors;
using CannoliKit.Processors.Core;
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
            services.AddCannoliProcessor<CannoliCleanupProcessor<TContext>, bool>(
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
            var commandNameAttribute = (CannoliCommandNameAttribute?)typeof(T)
                .GetCustomAttributes(typeof(CannoliCommandNameAttribute), true)
                .FirstOrDefault();

            if (commandNameAttribute == null)
            {
                throw new InvalidOperationException(
                    $"Type {nameof(T)} is missing {nameof(CannoliCommandNameAttribute)} and cannot be registered");
            }

            CannoliRegistry.Commands.TryAdd(
                commandNameAttribute.CommandName,
                typeof(T));

            services.AddTransient<ICannoliCommand, T>();

            return services;
        }

        public static IServiceCollection AddCannoliProcessor<THandler, TJob>(
            this IServiceCollection services, CannoliJobQueueOptions? options = null)
            where THandler : class, ICannoliProcessor<TJob>
        {
            services.AddSingleton<ICannoliJobQueue<TJob>>(sp =>
            {
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

                return new CannoliJobQueue<TJob>(
                    sp,
                    scopeFactory,
                    options ?? new CannoliJobQueueOptions { MaxConcurrentJobs = int.MaxValue });
            });

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
                    .GetCustomAttributes(typeof(CannoliCommandNameAttribute), true)
                    .FirstOrDefault();

                if (commandNameAttribute == null)
                {
                    throw new InvalidOperationException(
                        $"Type {command} is missing {nameof(CannoliCommandNameAttribute)} and cannot be registered");
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
