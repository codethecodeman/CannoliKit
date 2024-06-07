﻿using CannoliKit.Models;

namespace CannoliKit.Modules.Routing
{
    public sealed class CannoliRouteId
    {
        internal string RouteId { get; }
        internal CannoliRoute? Route { get; set; }

        internal CannoliRouteId(CannoliRoute route)
        {
            RouteId = route.Id;
            Route = route;
        }

        internal CannoliRouteId(string routeId)
        {
            RouteId = routeId;
        }

        public override string ToString()
        {
            if (Route == null) throw new ArgumentNullException(nameof(Route));
            return Route.Id;
        }

        public static implicit operator string(CannoliRouteId route)
        {
            return route.ToString();
        }
    }
}
