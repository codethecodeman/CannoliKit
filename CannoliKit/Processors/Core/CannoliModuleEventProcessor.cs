using CannoliKit.Concurrency;
using CannoliKit.Interfaces;
using CannoliKit.Workers.Jobs;

namespace CannoliKit.Processors.Core
{
    internal class CannoliModuleEventProcessor : ICannoliProcessor<CannoliModuleEventJob>
    {
        private readonly TurnManager _turnManager;
        private readonly ICannoliModuleRouter _router;

        public CannoliModuleEventProcessor(
            TurnManager turnManager,
            ICannoliModuleRouter router)
        {
            _turnManager = turnManager;
            _router = router;
        }

        public async Task HandleJobAsync(CannoliModuleEventJob job)
        {
            object paramToPass = null!;

            if (job.SocketMessageComponent != null)
            {
                paramToPass = job.SocketMessageComponent;
            }
            else if (job.SocketModal != null)
            {
                paramToPass = job.SocketModal;
            }

            if (job.Route.IsSynchronous == false)
            {
                await _router.RouteToModuleCallback(job.Route, paramToPass);

                return;
            }

            await ProcessJobInOrder(job, paramToPass);

            await Task.CompletedTask;
        }

        private async Task ProcessJobInOrder(CannoliModuleEventJob job, object parameter)
        {
            var thisTurn = new TaskCompletionSource<bool>();

            var previousTurn = _turnManager.GetTurnToAwait(job.Route.StateId, thisTurn);

            if (previousTurn != null)
            {
                await previousTurn.Task;
            }

            try
            {
                await _router.RouteToModuleCallback(job.Route, parameter);
            }
            catch (Exception ex)
            {
                //await EmitLog(new LogMessage(
                //    LogSeverity.Error,
                //    GetType().Name,
                //    ex.Message,
                //    ex));
            }
            finally
            {
                thisTurn.SetResult(true);
                _turnManager.CleanupTurns();
            }
        }
    }
}
