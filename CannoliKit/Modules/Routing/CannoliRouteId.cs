using CannoliKit.Models;

namespace CannoliKit.Modules.Routing
{
    /// <summary>
    /// Represents a Cannoli Route ID.
    /// </summary>
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

        /// <summary>
        /// Gets the string representation of the Cannoli Route ID.
        /// </summary>
        /// <returns>The string representation of the Cannoli Route ID.</returns>
        public override string ToString()
        {
            if (Route == null) throw new ArgumentNullException(nameof(Route));
            return Route.Id;
        }

        /// <summary>
        /// Implicitly converts a Cannoli Route ID to a string representation.
        /// </summary>
        /// <returns>The string representation of the Cannoli Route ID.</returns>
        public static implicit operator string(CannoliRouteId route)
        {
            return route.ToString();
        }
    }
}
