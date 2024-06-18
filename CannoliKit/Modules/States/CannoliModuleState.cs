using CannoliKit.Interfaces;
using CannoliKit.Modules.Routing;
using CannoliKit.Utilities;
using System.Text.Json.Serialization;

namespace CannoliKit.Modules.States
{
    /// <summary>
    /// Cannoli Module state base class.
    /// </summary>
    public class CannoliModuleState
    {
        /// <summary>
        /// State ID.
        /// </summary>
        public string Id { get; init; } = Guid.NewGuid().ToString();

        /// <summary>
        /// UTC datetime of state expiration. Default is +12 hours.
        /// </summary>
        public DateTime ExpiresOn { get; set; } = DateTime.UtcNow.AddHours(12);

        [JsonInclude]
        internal CannoliRouteId? CancelRoute { get; set; }

        [JsonInclude]
        internal Dictionary<string, CannoliRouteId> ReturnRoutes = [];

        internal ICannoliDbContext Db { get; set; } = null!;
        internal bool IsExpiringNow;
        internal bool IsSaved;

        /// <summary>
        /// Remove the state from the database.
        /// </summary>
        public async Task ExpireNowAsync()
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
