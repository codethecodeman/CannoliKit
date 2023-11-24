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
            _routesToAdd = new List<CannoliRoute>();
            State = state;
        }

        public CannoliRouteId CreateMessageComponentRoute(
            MessageComponentCallback callback,
            Priority priority = Priority.Normal,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            var route = CreateRoute(
                callback.Method,
                RouteType.MessageComponent,
                priority,
                parameter1,
                parameter2,
                parameter3);

            return new CannoliRouteId(route.RouteId);
        }

        public CannoliRouteId CreateModalRoute(
            ModalCallback callback,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            var route = CreateRoute(
                callback.Method,
                RouteType.Modal,
                Priority.High,
                parameter1,
                parameter2,
                parameter3);

            return new CannoliRouteId(route.RouteId);
        }

        private CannoliRoute CreateRoute(
            MemberInfo methodInfo,
            RouteType routeType,
            Priority priority,
            string? parameter1 = null,
            string? parameter2 = null,
            string? parameter3 = null)
        {
            var route = RouteUtility.CreateRoute(
                routeType: routeType,
                callbackType: _type.AssemblyQualifiedName!,
                callbackMethod: methodInfo.Name,
                stateId: State.Id,
                priority: priority,
                parameter1: parameter1,
                parameter2: parameter2,
                parameter3: parameter3);

            _routesToAdd.Add(route);

            return route;
        }

        internal void AddRoutes()
        {
            foreach (var route in _routesToAdd)
            {
                RouteUtility.AddRoute(_db, route);
            }

            _routesToAdd.Clear();
        }
    }
}
