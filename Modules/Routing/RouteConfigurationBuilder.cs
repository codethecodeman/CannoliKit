namespace CannoliKit.Modules.Routing
{
    public class RouteConfigurationBuilder
    {
        private CannoliRouteId? _cancellationRouteId;
        private readonly Dictionary<string, CannoliRouteId> _returnRouteIds = [];

        public RouteConfigurationBuilder() { }

        public RouteConfigurationBuilder WithCancellationRoute(CannoliRouteId routeId)
        {
            _cancellationRouteId = routeId;
            return this;
        }

        public RouteConfigurationBuilder WithReturnRoute(string tag, CannoliRouteId routeId)
        {
            _returnRouteIds.Add(tag, routeId);
            return this;
        }

        public RouteConfiguration Build()
        {
            return new RouteConfiguration
            {
                CancellationRouteId = _cancellationRouteId,
                ReturnRouteIds = _returnRouteIds
            };
        }
    }
}
