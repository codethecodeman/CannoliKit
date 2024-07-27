using CannoliKit.Concurrency;
using CannoliKit.Factories;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Processors.Jobs;
using CannoliKit.Utilities;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CannoliKit.Processors.Core
{
    internal sealed class CannoliModuleEventProcessor<TContext> : ICannoliProcessor<CannoliModuleEventJob>
    where TContext : DbContext, ICannoliDbContext
    {
        private readonly CannoliModuleTurnManager _cannoliModuleTurnManager;
        private readonly TContext _db;
        private readonly ICannoliModuleFactory _moduleFactory;
        private readonly ILogger<CannoliModuleEventProcessor<TContext>> _logger;
        private const string RouteFailedMessage = "Sorry, something unexpected happened. Please try again.";

        public CannoliModuleEventProcessor(
            CannoliModuleTurnManager cannoliModuleTurnManager,
            TContext db,
            ICannoliModuleFactory moduleFactory,
            ILogger<CannoliModuleEventProcessor<TContext>> logger)
        {
            _cannoliModuleTurnManager = cannoliModuleTurnManager;
            _db = db;
            _moduleFactory = moduleFactory;
            _logger = logger;
        }

        public async Task HandleJobAsync(CannoliModuleEventJob job)
        {
            SocketInteraction interaction = null!;
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
        }

        private async Task RouteToModuleCallback(CannoliRoute route, SocketInteraction interaction, SocketUser user)
        {
            var classType = ReflectionUtility.GetType(route.CallbackType)!;

            var callbackMethodInfo = ReflectionUtility.GetMethodInfo(classType, route.CallbackMethod)!;

            var target = ((CannoliModuleFactory)_moduleFactory).CreateModule(
                type: classType,
                requestingUser: user,
                interaction: interaction);

            await target.LoadModuleState(route);

            var callbackTask = (Task)callbackMethodInfo.Invoke(target, [interaction, route])!;
            await callbackTask;

            await target.SaveModuleState();
        }

        private async Task ProcessJobInOrder(CannoliModuleEventJob job, SocketInteraction interaction, SocketUser user)
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

                if (interaction.HasResponded == false)
                {
                    await interaction.DeferAsync();
                }

                await interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = RouteFailedMessage;
                    x.Components = null;
                    x.Embeds = null;
                });

                await SaveStateUtility.RemoveStateAsync(_db, job.Route.StateId);
            }
            finally
            {
                thisTurn.SetResult(true);
                _cannoliModuleTurnManager.CleanupTurns();
            }
        }
    }
}
