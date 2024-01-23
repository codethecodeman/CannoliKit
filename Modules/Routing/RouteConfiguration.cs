namespace CannoliKit.Modules.Routing
{
    public class RouteConfiguration
    {
        internal RouteConfiguration() { }

        internal CannoliRouteId? CancellationRouteId { get; init; }
        internal Dictionary<string, CannoliRouteId> ReturnRouteIds { get; init; } = [];
    }
}
