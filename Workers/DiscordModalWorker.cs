using DisCannoli.Enums;
using DisCannoli.Interfaces;
using DisCannoli.Utilities;
using DisCannoli.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Workers
{
    public class DiscordModalWorker<TContext> : DisCannoliWorker<TContext, DiscordModalJob> where TContext : DbContext, IDisCannoliDbContext
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

                await RouteUtility.RouteToModuleCallback(db, DisCannoliClient.DiscordClient, route, item.Modal);
            }
            catch (Exception ex)
            {
                await EmitLog(new LogMessage(
                    LogSeverity.Error,
                    GetType().Name,
                    ex.ToString(),
                    ex));
            }
        }
    }
}
