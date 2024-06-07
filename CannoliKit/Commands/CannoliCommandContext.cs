using Discord;
using Discord.WebSocket;

namespace CannoliKit.Commands
{
    public sealed class CannoliCommandContext
    {
        public SocketCommandBase Command { get; init; } = null!;
        public IApplicationCommandInteractionDataOption? SubCommandGroup { get; init; }
        public IApplicationCommandInteractionDataOption? SubCommand { get; init; }

    }
}
