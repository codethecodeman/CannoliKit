using CannoliKit.Interfaces;
using CannoliKit.Utilities;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Workers
{
    internal class CannoliCleanupWorker<TContext> : CannoliWorker<TContext, bool> where TContext : DbContext, ICannoliDbContext
    {
        internal CannoliCleanupWorker(int maxConcurrentTaskCount) : base(maxConcurrentTaskCount)
        {
        }

        protected override async Task DoWork(TContext db, DiscordSocketClient discordClient, bool item)
        {
            var expiredStates = await db.CannoliSaveStates
                .Where(s => s.ExpiresOn <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var expiredState in expiredStates)
            {
                await RouteUtility.RemoveRoutes(db, expiredState.Id);
                db.CannoliSaveStates.Remove(expiredState);
            }
        }
    }
}
