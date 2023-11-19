using CannoliKit.Modules.States;

namespace CannoliKit.Modules.Cancellation
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

        internal CannoliModuleState State { get; set; }

        public CancellationSettings(CannoliModuleState state)
        {
            IsEnabled = false;
            ButtonLabel = "Cancel";
            DoesDeleteCurrentState = true;
            State = state;
        }
    }
}
