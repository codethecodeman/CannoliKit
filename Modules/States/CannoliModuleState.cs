using CannoliKit.Modules.Routing;
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

        /// <summary>
        /// Remove the state from the database.
        /// </summary>
        public void ExpireNow()
        {
            ExpiresOn = DateTime.UtcNow;
        }
    }
}
