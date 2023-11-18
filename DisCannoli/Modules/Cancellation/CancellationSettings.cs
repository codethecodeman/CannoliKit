using DisCannoli.Modules.States;

namespace DisCannoli.Modules.Cancellation
{
    public class CancellationSettings
    {
        public bool IsEnabled { get; set; }
        public bool HasCustomRouting => CustomRouteId != null;
        public string ButtonLabel { get; set; }
        public bool DoesDeleteCurrentState { get; set; }
        internal string? CustomRouteId
        {
            get => State.CancelRouteId;
            set => State.CancelRouteId = value;
        }

        internal DisCannoliModuleState State { get; set; }

        public CancellationSettings(DisCannoliModuleState state)
        {
            IsEnabled = false;
            ButtonLabel = "Cancel";
            DoesDeleteCurrentState = true;
            State = state;
        }
    }
}
