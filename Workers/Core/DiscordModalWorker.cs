using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Workers.Core
{
    internal sealed class DiscordModalWorker<TContext> : CannoliWorker<TContext, DiscordModalJob> where TContext : DbContext, ICannoliDbContext
    {
        public DiscordModalWorker(int maxConcurrentTaskCount) : base(maxConcurrentTaskCount)
        {
        }

        protected override async Task DoWork(TContext db, DiscordSocketClient discordClient, DiscordModalJob item)
        {
            try
            {
                var route = await RouteUtility.GetRoute(db, RouteType.Modal, item.Modal.Data.CustomId);

                if (route == null) return;

                await RouteUtility.RouteToModuleCallback(db, CannoliClient.DiscordClient, route, item.Modal);
            }
            catch (Exception ex)
            {
                await EmitLog(new LogMessage(
                    LogSeverity.Error,
                    GetType().Name,
                    ex.Message,
                    ex));
            }
        }
    }
}
