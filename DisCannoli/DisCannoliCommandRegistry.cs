using DisCannoli.Commands;
using Discord;
using System.Collections.Concurrent;

namespace DisCannoli
{
    public class DisCannoliCommandRegistry
    {
        private readonly DisCannoliClient _cannoliClient;
        private readonly ConcurrentDictionary<Type, DisCannoliCommandBase> _commands;

        internal DisCannoliCommandRegistry(DisCannoliClient cannoliClient)
        {
            _commands = new ConcurrentDictionary<Type, DisCannoliCommandBase>();
            _cannoliClient = cannoliClient;
        }

        public void Add(DisCannoliCommandBase command)
        {
            if (_commands.ContainsKey(command.GetType())) return;

            command.Setup(_cannoliClient);
            _commands[command.GetType()] = command;
        }

        public T? GetCommand<T>() where T : DisCannoliCommandBase
        {
            return _commands.TryGetValue(typeof(T), out var worker)
                ? (T?)worker
                : null;
        }

        public DisCannoliCommandBase? GetCommand(string commandName)
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
