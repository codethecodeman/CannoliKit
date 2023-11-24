using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Workers.Core
{
    internal sealed class DiscordMessageComponentWorker<TContext> : CannoliWorker<TContext, DiscordMessageComponentJob> where TContext : DbContext, ICannoliDbContext
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

                await RouteUtility.RouteToModuleCallback(db, CannoliClient.DiscordClient, route, item.MessageComponent);
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
