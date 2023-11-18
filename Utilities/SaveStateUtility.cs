using DisCannoli.Interfaces;
using DisCannoli.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DisCannoli.Utilities
{
    internal static class SaveStateUtility
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        internal static async Task<T?> GetState<T>(IDisCannoliDbContext db, string stateId)
        {
            var savedState = await db.CannoliSaveStates
                .Where(g => g.Id == stateId)
                .Select(g => g.State)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (savedState == null)
            {
                return default;
            }

            var deserializedEntry = JsonSerializer.Deserialize<T>(
                Encoding.UTF8.GetString(savedState),
                JsonSerializerOptions
            );

            return deserializedEntry;
        }

        internal static async Task AddOrUpdateState(IDisCannoliDbContext db, string stateId, object payload, DateTime? expiresOn = null)
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
                entry = new SaveState()
                {
                    Id = stateId,
                    ExpiresOn = expiresOn,
                };

                db.CannoliSaveStates.Add(entry);
            }

            entry.State = Encoding.UTF8.GetBytes(serializedEntry);
            entry.UpdatedOn = DateTime.UtcNow;
        }

        internal static async Task RemoveState(IDisCannoliDbContext db, string stateId)
        {
            var savedState = await db.CannoliSaveStates
                .Where(g => g.Id == stateId)
                .FirstOrDefaultAsync();

            if (savedState != null)
            {
                db.CannoliSaveStates.Remove(savedState);
                await RouteUtility.RemoveRoutes(db, stateId);
            }
        }
    }
}
