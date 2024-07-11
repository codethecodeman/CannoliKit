using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Utilities
{
    internal static class RouteUtility
    {
        private const string RoutePrefix = "CannoliKit.Route.";

        internal static async Task<CannoliRoute?> GetRoute(ICannoliDbContext db, string id)
        {
            var route = db.CannoliRoutes.Local
                .FirstOrDefault(m =>
                    m.Id == id);

            if (route != null) return route;

            return await db.CannoliRoutes
                .FirstOrDefaultAsync(m =>
                    m.Id == id);
        }

        internal static async Task<CannoliRoute?> GetRoute(ICannoliDbContext db, string stateId, string routeName)
        {
            var route = db.CannoliRoutes.Local
                .FirstOrDefault(m =>
                    m.StateId == stateId
                    && m.Name == routeName);

            if (route != null) return route;

            return await db.CannoliRoutes
                .FirstOrDefaultAsync(m =>
                    m.StateId == stateId
                    && m.Name == routeName);
        }

        internal static async Task<CannoliRoute> CreateRoute(
            ICannoliDbContext db,
            RouteType routeType,
            string callbackType,
            string callbackMethod,
            string stateId,
            bool isSynchronous,
            bool isDeferred,
            string? routeName = null,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            CannoliRoute? route = null;

            if (routeName != null)
            {
                route = await GetRoute(db, stateId, routeName);
            }

            route ??= new CannoliRoute()
            {
                Id = $"{RoutePrefix}{Guid.NewGuid()}",
                Name = routeName,
                Type = routeType,
                CallbackType = callbackType,
                CallbackMethod = callbackMethod,
                StateId = stateId,
                IsSynchronous = isSynchronous,
                IsDeferred = isDeferred,
                Parameter1 = parameter1,
                Parameter2 = parameter2,
                Parameter3 = parameter3,
                IsNew = true,
            };

            return route;
        }

        internal static void AddRoute(ICannoliDbContext db, CannoliRoute route)
        {
            db.CannoliRoutes.Add(route);
        }

        internal static async Task RemoveRoutes(ICannoliDbContext db, string stateId, bool doForceRemoval = false)
        {
            var mappings = await db.CannoliRoutes
                .Where(m => m.StateId == stateId)
                .ToListAsync();

            if (mappings.Count > 0)
            {
                db.CannoliRoutes.RemoveRange(
                    mappings.Where(x => x.Name == null || doForceRemoval));
            }
        }

        internal static bool IsValidRouteId(string customId)
        {
            return customId.StartsWith(RoutePrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
