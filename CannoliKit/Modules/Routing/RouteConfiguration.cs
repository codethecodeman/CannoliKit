namespace CannoliKit.Modules.Routing
{
    public sealed class RouteConfiguration
    {
        internal RouteConfiguration(
            IReadOnlyDictionary<string, CannoliRouteId> returnRouteIds,
            CannoliRouteId? cancellationRouteId = null)
        {
            ReturnRouteIds = returnRouteIds;
            CancellationRouteId = cancellationRouteId;
        }

        internal RouteConfiguration()
        {
            ReturnRouteIds = new Dictionary<string, CannoliRouteId>();
        }

        public CannoliRouteId? CancellationRouteId { get; }
        public IReadOnlyDictionary<string, CannoliRouteId> ReturnRouteIds { get; }
    }
}
