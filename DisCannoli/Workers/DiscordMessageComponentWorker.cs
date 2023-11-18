using DisCannoli.Enums;
using DisCannoli.Interfaces;
using DisCannoli.Utilities;
using DisCannoli.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Workers
{
    internal class DiscordMessageComponentWorker<TContext> : DisCannoliWorker<TContext, DiscordMessageComponentJob> where TContext : DbContext, IDisCannoliDbContext
    {
        internal DiscordMessageComponentWorker(int maxConcurrentTaskCount) : base(maxConcurrentTaskCount)
        {
        }

        protected override async Task DoWork(TContext db, DiscordSocketClient discordClient, DiscordMessageComponentJob item)
        {
            try
            {
                var route = await RouteUtility.GetRoute(db, RouteType.MessageComponent, item.MessageComponent.Data.CustomId);

                if (route == null)
                {
                    await ShowExpiredMessage(item.MessageComponent);
                    return;
                }

                await RouteUtility.RouteToModuleCallback(db, DisCannoliClient.DiscordClient, route, item.MessageComponent);
            }
            catch (Exception ex)
            {
                await EmitLog(new LogMessage(
                    LogSeverity.Error,
                    GetType().Name,
                    ex.ToString(),
                    ex));

                await ShowExpiredMessage(item.MessageComponent);
            }
        }

        internal static async Task ShowExpiredMessage(SocketMessageComponent messageComponent)
        {
            await messageComponent.ModifyOriginalResponseAsync(m =>
            {
                m.Content = "Sorry. This interaction expired. Please try again.";
                m.Embeds = null;
                m.Components = null;
            });
        }
    }
}
