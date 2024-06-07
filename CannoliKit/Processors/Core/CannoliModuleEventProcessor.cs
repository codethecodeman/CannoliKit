using CannoliKit.Concurrency;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Utilities;
using CannoliKit.Workers.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace CannoliKit.Processors.Core
{
    internal sealed class CannoliModuleEventProcessor : ICannoliProcessor<CannoliModuleEventJob>
    {
        private readonly TurnManager _turnManager;
        private readonly IServiceProvider _serviceProvider;

        public CannoliModuleEventProcessor(
            TurnManager turnManager,
            IServiceProvider serviceProvider)
        {
            _turnManager = turnManager;
            _serviceProvider = serviceProvider;
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
                await RouteToModuleCallback(job.Route, paramToPass);

                return;
            }

            await ProcessJobInOrder(job, paramToPass);

            await Task.CompletedTask;
        }

        private async Task RouteToModuleCallback(CannoliRoute route, object parameter)
        {
            var classType = ReflectionUtility.GetType(route.CallbackType)!;

            var callbackMethodInfo = ReflectionUtility.GetMethodInfo(classType, route.CallbackMethod)!;

            var target = (ICannoliModule)_serviceProvider.GetRequiredService(classType);
            await target.LoadModuleState(route);

            var callbackTask = (Task)callbackMethodInfo.Invoke(target, [parameter, route])!;
            await callbackTask;

            await target.SaveModuleState();
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
                await RouteToModuleCallback(job.Route, parameter);
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
