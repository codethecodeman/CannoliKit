using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace CannoliKit.Utilities
{
    internal static class RouteUtility
    {
        private const string RoutePrefix = "CannoliKit.Route.";
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _sequentialExecutionLocks = new();

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

        internal static async Task<CannoliRoute?> GetRoute(ICannoliDbContext db, RouteType routeType, string id)
        {
            var route = db.CannoliRoutes.Local
                .FirstOrDefault(m =>
                    m.Type == routeType
                    && m.Id == id);

            if (route != null) return route;

            return await db.CannoliRoutes
                .FirstOrDefaultAsync(m =>
                    m.Type == routeType
                    && m.Id == id);
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
            Priority priority,
            bool isSynchronous,
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
                Priority = priority,
                IsSynchronous = isSynchronous,
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

        internal static async Task RouteToModuleCallback(
            ICannoliDbContext db,
            DiscordSocketClient discordClient,
            CannoliRoute route,
            object parameter)
        {
            if (route.StateIdToBeDeleted != null)
            {
                await SaveStateUtility.RemoveState(db, route.StateIdToBeDeleted);
            }

            var classType = ReflectionUtility.GetType(route.CallbackType)!;

            var callbackMethodInfo = ReflectionUtility.GetMethodInfo(classType, route.CallbackMethod)!;
            var loadStateMethodInfo = ReflectionUtility.GetMethodInfo(classType, "LoadModuleState")!;
            var saveStateMethodInfo = ReflectionUtility.GetMethodInfo(classType, "SaveModuleState")!;

            var target = Activator.CreateInstance(classType, [db, discordClient, null]);

            var loadStateTask = (Task)loadStateMethodInfo.Invoke(target, [route.StateId])!;
            await loadStateTask;

            var callbackTask = (Task)callbackMethodInfo.Invoke(target, [parameter, route])!;
            await callbackTask;

            var saveStateTask = (Task)saveStateMethodInfo.Invoke(target, null)!;
            await saveStateTask;

        }
    }
}
