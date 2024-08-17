using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Modules.Routing;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Utilities
{
    /// <summary>
    /// Utilities for Cannoli Routes.
    /// </summary>
    public static class RouteUtility
    {
        private const string RoutePrefix = "CannoliKit.Route.";

        /// <summary>
        /// Set a Cannoli Route's parameters.
        /// </summary>
        /// <param name="routeId">Route ID.</param>
        /// <param name="parameter1">Parameter 1.</param>
        /// <param name="parameter2">Parameter 2.</param>
        /// <param name="parameter3">Parameter 3.</param>
        public static void SetParameters(
            CannoliRouteId routeId,
            string parameter1,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            routeId.Route!.Parameter1 = parameter1;
            routeId.Route!.Parameter2 = parameter2;
            routeId.Route!.Parameter3 = parameter3;
        }

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
                IsNew = true,
            };

            route.Parameter1 = parameter1;
            route.Parameter2 = parameter2;
            route.Parameter3 = parameter3;

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
