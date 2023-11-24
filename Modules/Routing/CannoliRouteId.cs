namespace CannoliKit.Modules.Routing
{
    public class CannoliRouteId
    {
        private readonly string _routeId;
        internal CannoliRouteId(string routeId)
        {
            _routeId = routeId;
        }

        public override string ToString()
        {
            return _routeId;
        }

        public static implicit operator string(CannoliRouteId route)
        {
            return route.ToString();
        }
    }
}
