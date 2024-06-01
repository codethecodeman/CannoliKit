using Discord;
using Discord.WebSocket;

namespace CannoliKit.Commands
{
    public class CannoliCommandContext
    {
        public SocketCommandBase Command { get; init; } = null!;
        public IApplicationCommandInteractionDataOption? SubCommandGroup { get; init; }
        public IApplicationCommandInteractionDataOption? SubCommand { get; init; }

    }
}
