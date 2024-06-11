using CannoliKit.Modules.Routing;
using CannoliKit.Modules.States;

namespace CannoliKit.Modules.Cancellation
{
    /// <summary>
    /// Handles cancellation settings for a Cannoli Module.
    /// </summary>
    public sealed class CancellationSettings
    {
        /// <summary>
        /// Indicates if cancellation is enabled. Default value is false.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Indicates if cancellation will route to a custom Cannoli Route. E.g., back to another Cannoli Module.
        /// </summary>
        public bool HasCustomRouting => Route != null;

        /// <summary>
        /// Label to display on the cancellation button. Default value is "Cancel".
        /// </summary>
        public string ButtonLabel { get; set; } = "Cancel";

        /// <summary>
        /// Custom cancellation Cannoli Route, if set.
        /// </summary>
        public CannoliRouteId? Route => State.CancelRoute;

        internal CannoliModuleState State { get; set; }

        internal CancellationSettings(CannoliModuleState state)
        {
            State = state;
        }

        /// <summary>
        /// Set a custom cancellation Cannoli Route.
        /// </summary>
        /// <param name="routeId">Cannoli Route ID</param>
        public void SetRoute(CannoliRouteId routeId)
        {
            State.CancelRoute = routeId;
        }
    }
}
