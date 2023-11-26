using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Utilities
{
    public static class RouteUtility
    {
        private const string RoutePrefix = "CannoliKit.Route.";

        internal static async Task<CannoliRoute?> GetRoute(ICannoliDbContext db, string id)
        {
            var route = db.CannoliRoutes.Local
                .FirstOrDefault(m =>
                    m.RouteId == id);

            if (route != null) return route;

            return await db.CannoliRoutes
                .FirstOrDefaultAsync(m =>
                    m.RouteId == id);
        }

        internal static async Task<CannoliRoute?> GetRoute(ICannoliDbContext db, RouteType routeType, string id)
        {
            var route = db.CannoliRoutes.Local
                .FirstOrDefault(m =>
                    m.Type == routeType
                    && m.RouteId == id);

            if (route != null) return route;

            return await db.CannoliRoutes
                .FirstOrDefaultAsync(m =>
                    m.Type == routeType
                    && m.RouteId == id);
        }

        internal static CannoliRoute CreateRoute(
            RouteType routeType,
            string callbackType,
            string callbackMethod,
            string stateId,
            Priority priority,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            var route = new CannoliRoute()
            {
                RouteId = $"{RoutePrefix}{Guid.NewGuid()}",
                Type = routeType,
                CallbackType = callbackType,
                CallbackMethod = callbackMethod,
                StateId = stateId,
                Priority = priority,
                Parameter1 = parameter1,
                Parameter2 = parameter2,
                Parameter3 = parameter3,
            };

            return route;
        }

        internal static void AddRoute(ICannoliDbContext db, CannoliRoute route)
        {
            db.CannoliRoutes.Add(route);
        }

        internal static async Task RemoveRoutes(ICannoliDbContext db, string stateId)
        {
            var mappings = await db.CannoliRoutes
                .Where(m => m.StateId == stateId)
                .ToListAsync();

            if (mappings.Count != 0)
            {
                db.CannoliRoutes.RemoveRange(mappings);
            }
        }

        internal static bool IsValidRouteId(string customId)
        {
            return customId.StartsWith(RoutePrefix, StringComparison.OrdinalIgnoreCase);
        }

        internal static async Task RouteToModuleCallback(
            ICannoliDbContext db,
            DiscordSocketClient discordClient,
            CannoliRoute route,
            object parameter)
        {
            await RemoveRoutes(db, route.StateId);

            if (route.StateIdToBeDeleted != null)
            {
                await SaveStateUtility.RemoveState(db, route.StateIdToBeDeleted);
            }

            var classType = ReflectionUtility.GetType(route.CallbackType)!;

            var callbackMethodInfo = ReflectionUtility.GetMethodInfo(classType, route.CallbackMethod)!;
            var loadStateMethodInfo = ReflectionUtility.GetMethodInfo(classType, "LoadModuleState")!;
            var target = Activator.CreateInstance(classType, db, discordClient);

            var loadStateTask = (Task)loadStateMethodInfo.Invoke(target, new object?[] { route.StateId })!;
            await loadStateTask;

            var callbackTask = (Task)callbackMethodInfo.Invoke(target, new[] { parameter, route })!;
            await callbackTask;
        }
    }
}
