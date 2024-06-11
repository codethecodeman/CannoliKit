using CannoliKit.Interfaces;

namespace CannoliKit.Modules.Routing
{
    /// <summary>
    /// Builds a new <see cref="RouteConfiguration"/> to be passed to a <see cref="ICannoliModuleFactory"/>.
    /// </summary>
    public sealed class RouteConfigurationBuilder
    {
        private CannoliRouteId? _cancellationRouteId;
        private readonly Dictionary<string, CannoliRouteId> _returnRouteIds = [];

        /// <summary>
        /// Add a cancellation Cannoli Route. Subsequent calls will overwrite. 
        /// </summary>
        /// <param name="routeId">Cannoli Route ID.</param>
        public RouteConfigurationBuilder WithCancellationRoute(CannoliRouteId routeId)
        {
            _cancellationRouteId = routeId;
            return this;
        }

        /// <summary>
        /// Add a return Cannoli Route. Subsequent calls with the same tag will overwrite. 
        /// </summary>
        /// <param name="tag">Tag name to be used for reference.</param>
        /// <param name="routeId">Cannoli Route ID.</param>
        public RouteConfigurationBuilder WithReturnRoute(string tag, CannoliRouteId routeId)
        {
            _returnRouteIds.Add(tag, routeId);
            return this;
        }

        /// <summary>
        /// Build the resulting <see cref="RouteConfiguration"/>.
        /// </summary>
        /// <returns>New instance of the resulting <see cref="RouteConfiguration"/>.</returns>
        public RouteConfiguration Build()
        {
            return new RouteConfiguration(
                _returnRouteIds,
                _cancellationRouteId);
        }
    }
}
