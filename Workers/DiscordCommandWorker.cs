using DisCannoli.Interfaces;
using DisCannoli.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Workers
{
    internal class DiscordCommandWorker<TContext> : DisCannoliWorker<TContext, DiscordCommandJob> where TContext : DbContext, IDisCannoliDbContext
    {
        internal DiscordCommandWorker(int maxConcurrentTaskCount) : base(maxConcurrentTaskCount)
        {
        }

        protected override async Task DoWork(TContext db, DiscordSocketClient discordClient, DiscordCommandJob item)
        {
            var commandName = item.SocketCommand.CommandName;

            await EmitLog(new LogMessage(
                LogSeverity.Info,
                GetType().Name,
                $"Received command {commandName} from {item.SocketCommand.User.Username}"));

            var command = DisCannoliClient.Commands.GetCommand(commandName);

            if (command == null) return;

            await command.Respond(db, DisCannoliClient.DiscordClient, item.SocketCommand);
        }
    }
}
