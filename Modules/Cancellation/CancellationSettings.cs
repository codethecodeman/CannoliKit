using CannoliKit.Modules.Routing;
using CannoliKit.Modules.States;

namespace CannoliKit.Modules.Cancellation
{
    public sealed class CancellationSettings
    {
        public bool IsEnabled { get; set; }
        public bool HasCustomRouting => CustomRoute != null;
        public string ButtonLabel { get; set; }
        public bool DoesDeleteCurrentState { get; set; }
        internal CannoliRouteId? CustomRoute
        {
            get => State.CancelRoute;
            set => State.CancelRoute = value;
        }

        internal CannoliModuleState State { get; set; }

        internal CancellationSettings(CannoliModuleState state)
        {
            IsEnabled = false;
            ButtonLabel = "Cancel";
            DoesDeleteCurrentState = true;
            State = state;
        }
    }
}
