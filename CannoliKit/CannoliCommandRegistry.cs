using CannoliKit.Commands;
using CannoliKit.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace CannoliKit
{
    internal class CannoliCommandRegistry
    {
        internal ConcurrentDictionary<string, CannoliCommandMeta> Commands { get; } = [];
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private bool _isLoaded;

        public CannoliCommandRegistry(
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        internal async Task LoadCommands()
        {
            if (_isLoaded) return;

            using var scope = _serviceScopeFactory.CreateScope();
            var commands = scope.ServiceProvider.GetServices<ICannoliCommand>();

            foreach (var command in commands)
            {
                Commands[command.Name] = new CannoliCommandMeta
                {
                    Name = command.Name,
                    DeferralType = command.DeferralType,
                    ApplicationCommandProperties = await command.BuildAsync(),
                    Type = command.GetType()
                };
            }

            _isLoaded = true;
        }
    }
}
