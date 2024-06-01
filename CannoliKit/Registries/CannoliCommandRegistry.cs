using CannoliKit.Commands;
using CannoliKit.Interfaces;
using Discord;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace CannoliKit.Registries
{
    public sealed class CannoliCommandRegistry<TContext>
    where TContext : DbContext, ICannoliDbContext
    {
        private readonly CannoliClient<TContext> _cannoliClient;
        private readonly ConcurrentDictionary<Type, CannoliCommand<TContext>> _commands;

        internal CannoliCommandRegistry(CannoliClient<TContext> cannoliClient)
        {
            _commands = new ConcurrentDictionary<Type, CannoliCommand<TContext>>();
            _cannoliClient = cannoliClient;
        }

        public void Add(CannoliCommand<TContext> command)
        {
            if (_commands.ContainsKey(command.GetType())) return;
            _commands[command.GetType()] = command;
        }

        public T? GetCommand<T>() where T : CannoliCommand<TContext>
        {
            return _commands.TryGetValue(typeof(T), out var worker)
                ? (T?)worker
                : null;
        }

        public CannoliCommand<TContext>? GetCommand(string commandName)
        {
            var command = _commands.Values.FirstOrDefault(c => c.Name == commandName);

            return command;
        }

        public async Task Register()
        {
            var remoteGlobalCommands = await _cannoliClient.DiscordClient.GetGlobalApplicationCommandsAsync();
            var localGlobalCommands = _commands.Values.ToList();

            foreach (var globalCommand in remoteGlobalCommands)
            {
                if (localGlobalCommands.Any(c => c.Name == globalCommand.Name))
                {
                    continue;
                }

                await globalCommand.DeleteAsync();

                await _cannoliClient.EmitLog(new LogMessage(
                    LogSeverity.Info,
                    GetType().Name,
                    $"Deleted global command {globalCommand.Name}"));
            }

            await _cannoliClient.DiscordClient.BulkOverwriteGlobalApplicationCommandsAsync(
                localGlobalCommands.Select(c => c.Build()).ToArray());

            foreach (var localCommand in localGlobalCommands)
            {
                await _cannoliClient.EmitLog(new LogMessage(
                    LogSeverity.Info,
                    GetType().Name,
                    $"Added global command {localCommand.Name}"));
            }
        }
    }
}
