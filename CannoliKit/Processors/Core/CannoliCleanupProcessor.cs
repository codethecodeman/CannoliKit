using CannoliKit.Interfaces;
using CannoliKit.Processors.Jobs;
using CannoliKit.Utilities;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Processors.Core
{
    internal sealed class CannoliCleanupProcessor<TContext> : ICannoliProcessor<CannoliCleanupJob>
    where TContext : DbContext, ICannoliDbContext
    {
        private readonly TContext _db;

        public CannoliCleanupProcessor(
            TContext db)
        {
            _db = db;
        }

        public async Task HandleJobAsync(CannoliCleanupJob job)
        {
            var expiredStates = await _db.CannoliSaveStates
                .Where(s => s.ExpiresOn <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var expiredState in expiredStates)
            {
                await RouteUtility.RemoveRoutes(
                    _db,
                    expiredState.Id,
                    true);

                _db.CannoliSaveStates.Remove(expiredState);
            }
        }
    }
}
