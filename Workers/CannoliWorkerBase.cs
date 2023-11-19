using Discord;

namespace CannoliKit.Workers
{
    public abstract class CannoliWorkerBase
    {
        internal delegate Task LogEventHandler(LogMessage e);
        internal event LogEventHandler? Log;

        internal abstract void Setup(CannoliClient cannoliClient);

        protected async Task EmitLog(LogMessage logMessage)
        {
            if (Log == null) return;

            await Log.Invoke(logMessage);
        }
    }
}
