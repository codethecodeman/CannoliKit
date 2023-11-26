using CannoliKit.Models;

namespace CannoliKit.Modules.Routing
{
    public class CannoliRouteId
    {
        internal string RouteId { get; }
        internal CannoliRoute? Route { get; set; }

        internal CannoliRouteId(CannoliRoute route)
        {
            RouteId = route.RouteId;
            Route = route;
        }

        internal CannoliRouteId(string routeId)
        {
            RouteId = routeId;
        }

        public override string ToString()
        {
            if (Route == null) throw new ArgumentNullException(nameof(Route));
            return Route.RouteId;
        }

        public static implicit operator string(CannoliRouteId route)
        {
            return route.ToString();
        }
    }
}
