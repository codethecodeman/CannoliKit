using DisCannoli.Interfaces;
using DisCannoli.Utilities;
using System.Text.Json.Serialization;

namespace DisCannoli.Modules.States
{
    public class DisCannoliModuleState
    {
        public string Id { get; init; }

        public string? InfoMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime ExpiresOn { get; set; }

        [JsonInclude]
        internal string? CancelRouteId { get; set; }

        internal bool DidSaveAtLeastOnce { get; private set; }

        internal IDisCannoliDbContext Db { get; set; } = null!;

        internal event EventHandler? OnSave;

        public DisCannoliModuleState()
        {
            Id = Guid.NewGuid().ToString();
            ExpiresOn = DateTime.UtcNow.AddHours(1);
        }

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
