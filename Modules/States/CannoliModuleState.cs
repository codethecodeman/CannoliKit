using CannoliKit.Interfaces;
using CannoliKit.Modules.Routing;
using CannoliKit.Utilities;
using System.Text.Json.Serialization;

namespace CannoliKit.Modules.States
{
    public class CannoliModuleState
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();

        public string? InfoMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime ExpiresOn { get; set; } = DateTime.UtcNow.AddHours(1);

        [JsonInclude]
        internal CannoliRouteId? CancelRoute { get; set; }

        [JsonInclude]
        internal Dictionary<string, CannoliRouteId> ReturnRoutes = new();

        internal ICannoliDbContext Db { get; set; } = null!;
        internal bool IsExpiringNow;
        internal bool IsSaved;

        /// <summary>
        /// Remove the state from the database.
        /// </summary>
        public async Task ExpireNow()
        {
            IsExpiringNow = true;
            await SaveStateUtility.RemoveState(Db, Id);
        }

        internal async Task Save()
        {
            await SaveStateUtility.AddOrUpdateState(Db, Id, this, ExpiresOn);
            IsSaved = true;
        }
    }
}
