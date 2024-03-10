using CannoliKit.Interfaces;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Workers
{
    public abstract class CannoliWorkerBase<TContext>
        where TContext : DbContext, ICannoliDbContext
    {
        internal delegate Task LogEventHandler(LogMessage e);
        internal event LogEventHandler? Log;

        internal CannoliWorkerBase() { }

        internal abstract void Setup(CannoliClient<TContext> cannoliClient);

        protected async Task EmitLog(LogMessage logMessage)
        {
            if (Log == null) return;

            await Log.Invoke(logMessage);
        }
    }
}
