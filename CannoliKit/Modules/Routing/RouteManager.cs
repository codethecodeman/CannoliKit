using CannoliKit.Attributes;
using CannoliKit.Enums;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Modules.States;
using CannoliKit.Utilities;
using Discord.WebSocket;
using System.Reflection;

namespace CannoliKit.Modules.Routing
{
    public sealed class RouteManager
    {
        public delegate Task MessageComponentCallback(SocketMessageComponent messageComponent, CannoliRoute route);
        public delegate Task ModalCallback(SocketModal modal, CannoliRoute route);

        internal CannoliModuleState State { get; set; }

        private readonly ICannoliDbContext _db;
        private readonly Type _type;
        private readonly List<CannoliRoute> _routesToAdd;

        internal RouteManager(ICannoliDbContext db, Type type, CannoliModuleState state)
        {
            _db = db;
            _type = type;
            _routesToAdd = [];
            State = state;
        }

        public async Task<CannoliRouteId> CreateMessageComponentRoute(
            MessageComponentCallback callback,
            bool isDeferred = true,
            string? routeName = null,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            var route = await CreateRoute(
                callback.Method,
                RouteType.MessageComponent,
                isDeferred,
                routeName,
                parameter1,
                parameter2,
                parameter3);

            return new CannoliRouteId(route);
        }

        public async Task<CannoliRouteId> CreateModalRoute(
            ModalCallback callback,
            string? routeName = null,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            var route = await CreateRoute(
                callback.Method,
                RouteType.Modal,
                false,
                routeName,
                parameter1,
                parameter2,
                parameter3);

            return new CannoliRouteId(route);
        }

        private async Task<CannoliRoute> CreateRoute(
            MemberInfo methodInfo,
            RouteType routeType,
            bool isDeferred,
            string? routeName = null,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            var isSynchronous = methodInfo
                .GetCustomAttributes(typeof(ParallelExecutionAttribute), inherit: true)
                .Length == 0;

            var route = await RouteUtility.CreateRoute(
                db: _db,
                routeType: routeType,
                callbackType: _type.AssemblyQualifiedName!,
                callbackMethod: methodInfo.Name,
                stateId: State.Id,
                isSynchronous: isSynchronous,
                routeName: routeName,
                isDeferred: isDeferred,
                parameter1: parameter1,
                parameter2: parameter2,
                parameter3: parameter3);

            if (route.IsNew)
            {
                _routesToAdd.Add(route);
            }

            return route;
        }

        internal void AddRoutes()
        {
            foreach (var route in _routesToAdd)
            {
                RouteUtility.AddRoute(_db, route);
            }
        }
    }
}
