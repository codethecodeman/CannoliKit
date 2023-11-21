using CannoliKit.Commands;
using Discord;
using System.Collections.Concurrent;

namespace CannoliKit.Registries
{
    public class CannoliCommandRegistry
    {
        private readonly CannoliClient _cannoliClient;
        private readonly ConcurrentDictionary<Type, CannoliCommandBase> _commands;

        internal CannoliCommandRegistry(CannoliClient cannoliClient)
        {
            _commands = new ConcurrentDictionary<Type, CannoliCommandBase>();
            _cannoliClient = cannoliClient;
        }

        public void Add(CannoliCommandBase command)
        {
            if (_commands.ContainsKey(command.GetType())) return;

            command.Setup(_cannoliClient);
            _commands[command.GetType()] = command;
        }

        public T? GetCommand<T>() where T : CannoliCommandBase
        {
            return _commands.TryGetValue(typeof(T), out var worker)
                ? (T?)worker
                : null;
        }

        public CannoliCommandBase? GetCommand(string commandName)
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
