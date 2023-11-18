using Discord;

namespace DisCannoli.Workers
{
    public abstract class DisCannoliWorkerBase
    {
        internal delegate Task LogEventHandler(LogMessage e);
        internal event LogEventHandler? Log;

        internal abstract void Setup(DisCannoliClient disCannoliClient);

        protected async Task EmitLog(LogMessage logMessage)
        {
            if (Log == null) return;

            await Log.Invoke(logMessage);
        }
    }
}
