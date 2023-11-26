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

        public CannoliRouteId? CancelRoute { get; internal set; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, CannoliRouteId> ReturnRoutes => SecuredReturnRoutes;

        [JsonInclude]
        internal Dictionary<string, CannoliRouteId> SecuredReturnRoutes = new();

        internal bool DidSaveAtLeastOnce { get; private set; }

        internal ICannoliDbContext Db { get; set; } = null!;

        internal event EventHandler? OnSave;

        /// <summary>
        /// Add or update the state in the database.
        /// </summary>
        public async Task Save()
        {
            await SaveStateUtility.AddOrUpdateState(Db, Id, this, ExpiresOn);
            OnSave?.Invoke(this, EventArgs.Empty);
            DidSaveAtLeastOnce = true;
        }

        /// <summary>
        /// Remove the state from the database.
        /// </summary>
        public async Task Delete()
        {
            await SaveStateUtility.RemoveState(Db, Id);
            DidSaveAtLeastOnce = false;
        }
    }
}
