using CannoliKit.Converters;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Modules.States;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CannoliKit.Utilities
{
    /// <summary>
    /// Utilities for Cannoli Save States.
    /// </summary>
    public static class SaveStateUtility
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions;

        static SaveStateUtility()
        {
            JsonSerializerOptions = new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            JsonSerializerOptions.Converters.Add(new CannoliRouteIdJsonConverter());
        }

        internal static async Task<T?> GetState<T>(ICannoliDbContext db, string stateId)
            where T : class
        {
            var savedState = await db.CannoliSaveStates
                .Where(g => g.Id == stateId)
                .Select(g => g.State)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (savedState == null)
            {
                return null;
            }

            var deserializedEntry = JsonSerializer.Deserialize<T>(
                Encoding.UTF8.GetString(savedState),
                JsonSerializerOptions
            );

            if (deserializedEntry is CannoliModuleState state)
            {
                state.Id = stateId;
            }

            return deserializedEntry;
        }

        internal static async Task AddOrUpdateState(ICannoliDbContext db, string stateId, object payload, DateTime? expiresOn = null)
        {
            var serializedEntry = JsonSerializer.Serialize(
                payload,
                JsonSerializerOptions
            );

            var entry = db.CannoliSaveStates.Local.FirstOrDefault(g =>
                g.Id == stateId);

            entry ??= await db.CannoliSaveStates.FirstOrDefaultAsync(g =>
                    g.Id == stateId);

            if (entry == null)
            {
                entry = new CannoliSaveState()
                {
                    Id = stateId,
                    ExpiresOn = expiresOn,
                };

                db.CannoliSaveStates.Add(entry);
            }

            entry.ExpiresOn = expiresOn;
            entry.State = Encoding.UTF8.GetBytes(serializedEntry);
            entry.UpdatedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Remove Cannoli Save State from database.
        /// </summary>
        /// <param name="db">Database.</param>
        /// <param name="stateId">Save State ID.</param>
        public static async Task RemoveStateAsync(ICannoliDbContext db, string stateId)
        {
            var savedState = await db.CannoliSaveStates
                .Where(g => g.Id == stateId)
                .FirstOrDefaultAsync();

            if (savedState != null)
            {
                db.CannoliSaveStates.Remove(savedState);
                await RouteUtility.RemoveRoutes(db, stateId, doForceRemoval: true);
            }
        }
    }
}
