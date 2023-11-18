using DisCannoli.Interfaces;
using DisCannoli.Utilities;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Workers
{
    internal class DisCannoliCleanupWorker<TContext> : DisCannoliWorker<TContext, bool> where TContext : DbContext, IDisCannoliDbContext
    {
        internal DisCannoliCleanupWorker(int maxConcurrentTaskCount) : base(maxConcurrentTaskCount)
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
