using CannoliKit.Interfaces;
using CannoliKit.Utilities;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Workers.Core
{
    internal sealed class CannoliCleanupWorker<TContext> : CannoliWorker<TContext, bool> where TContext : DbContext, ICannoliDbContext
    {
        internal CannoliCleanupWorker() : base(maxConcurrentTaskCount: 1)
        {
        }

        protected override async Task DoWork(TContext db, DiscordSocketClient discordClient, bool item)
        {
            var expiredStates = await db.CannoliSaveStates
                .Where(s => s.ExpiresOn <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var expiredState in expiredStates)
            {
                await RouteUtility.RemoveRoutes(
                    db,
                    expiredState.Id,
                    true);

                db.CannoliSaveStates.Remove(expiredState);
            }
        }
    }
}
