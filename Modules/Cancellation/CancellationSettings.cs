using CannoliKit.Modules.Routing;
using CannoliKit.Modules.States;

namespace CannoliKit.Modules.Cancellation
{
    public sealed class CancellationSettings
    {
        public bool IsEnabled { get; set; }
        public bool HasCustomRouting => Route != null;
        public string ButtonLabel { get; set; }
        internal CannoliRouteId? Route => State.CancelRoute;
        internal CannoliModuleState State { get; set; }

        internal CancellationSettings(CannoliModuleState state)
        {
            IsEnabled = false;
            ButtonLabel = "Cancel";
            State = state;
        }

        public void SetCancelRoute(CannoliRouteId routeId)
        {
            State.CancelRoute = routeId;
        }
    }
}
