﻿using CannoliKit.Interfaces;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Workers.Core
{
    internal class DiscordCommandWorker<TContext> : CannoliWorker<TContext, DiscordCommandJob> where TContext : DbContext, ICannoliDbContext
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

            var command = CannoliClient.Commands.GetCommand(commandName);

            if (command == null) return;

            await command.Respond(db, CannoliClient.DiscordClient, item.SocketCommand);
        }
    }
}