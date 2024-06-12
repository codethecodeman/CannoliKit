using CannoliKit.Concurrency;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Processors.Jobs;
using CannoliKit.Utilities;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace CannoliKit.Processors.Core
{
    internal sealed class CannoliModuleEventProcessor : ICannoliProcessor<CannoliModuleEventJob>
    {
        private readonly CannoliModuleTurnManager _cannoliModuleTurnManager;
        private readonly ICannoliModuleFactory _moduleFactory;
        private readonly ILogger<CannoliModuleEventProcessor> _logger;

        public CannoliModuleEventProcessor(
            CannoliModuleTurnManager cannoliModuleTurnManager,
            ICannoliModuleFactory moduleFactory,
            ILogger<CannoliModuleEventProcessor> logger)
        {
            _cannoliModuleTurnManager = cannoliModuleTurnManager;
            _moduleFactory = moduleFactory;
            _logger = logger;
        }

        public async Task HandleJobAsync(CannoliModuleEventJob job)
        {
            object interaction = null!;
            SocketUser requestingUser = null!;

            if (job.SocketMessageComponent != null)
            {
                interaction = job.SocketMessageComponent;
                requestingUser = job.SocketMessageComponent.User;
            }
            else if (job.SocketModal != null)
            {
                interaction = job.SocketModal;
                requestingUser = job.SocketModal.User;
            }

            if (job.Route.IsSynchronous == false)
            {
                await RouteToModuleCallback(job.Route, interaction, requestingUser);

                return;
            }

            await ProcessJobInOrder(job, interaction, requestingUser);

            await Task.CompletedTask;
        }

        private async Task RouteToModuleCallback(CannoliRoute route, object interaction, SocketUser user)
        {
            var classType = ReflectionUtility.GetType(route.CallbackType)!;

            var callbackMethodInfo = ReflectionUtility.GetMethodInfo(classType, route.CallbackMethod)!;

            var target = _moduleFactory.CreateModule(
                classType,
                user);

            await target.LoadModuleState(route);

            var callbackTask = (Task)callbackMethodInfo.Invoke(target, [interaction, route])!;
            await callbackTask;

            await target.SaveModuleState();
        }

        private async Task ProcessJobInOrder(CannoliModuleEventJob job, object interaction, SocketUser user)
        {
            var thisTurn = new TaskCompletionSource<bool>();

            var previousTurn = _cannoliModuleTurnManager.GetTurnToAwait(job.Route.StateId, thisTurn);

            if (previousTurn != null)
            {
                await previousTurn.Task;
            }

            try
            {
                await RouteToModuleCallback(job.Route, interaction, user);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Failed while executing Cannoli Module Route with ID {routeId}. {message}",
                    job.Route.Id,
                    ex.Message);
            }
            finally
            {
                thisTurn.SetResult(true);
                _cannoliModuleTurnManager.CleanupTurns();
            }
        }
    }
}
