using CannoliKit.Exceptions;
using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Modules.Routing;
using CannoliKit.Modules.States;
using CannoliKit.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Modules
{
    /// <summary>
    /// Represents a Cannoli Module. Multi-featured UI modules with a state and interactions that are persisted to a database.
    /// Cannoli Modules are not directly registered as services. Instead, they must be created using <see cref="ICannoliModuleFactory"/>.
    /// </summary>
    /// <typeparam name="TContext"><see cref="DbContext"/> that implements <see cref="ICannoliDbContext"/>.</typeparam>
    /// <typeparam name="TState">Type that implements <see cref="CannoliModuleState"/> and has a parameterless constructor.</typeparam>
    public abstract class CannoliModule<TContext, TState> : CannoliModuleBase
        where TContext : DbContext, ICannoliDbContext
        where TState : CannoliModuleState, new()
    {
        /// <summary>
        /// Discord client.
        /// </summary>
        protected readonly DiscordSocketClient DiscordClient;

        /// <summary>
        /// Database. Will be automatically saved after callbacks have executed.
        /// </summary>
        protected readonly TContext Db;

        /// <summary>
        /// Pagination settings and utility.
        /// </summary>
        protected readonly Pagination.Pagination Pagination;

        /// <summary>
        /// Cannoli Route utility.
        /// </summary>
        protected readonly RouteManager RouteManager;

        /// <summary>
        /// Cancellation settings.
        /// </summary>
        protected readonly Cancellation.CancellationSettings Cancellation;

        /// <summary>
        /// Alert message settings.
        /// </summary>
        protected readonly Alerts.AlertsSettings Alerts;

        /// <summary>
        /// User that initiated the interaction in the current context.
        /// </summary>
        protected readonly SocketUser User;

        /// <summary>
        /// Module state. Will be automatically persisted to the database.
        /// </summary>
        protected TState State { get; private set; }

        /// <summary>
        /// Cannoli Routes that have been passed in from a referring module as a means to pass information to or return to a different module.
        /// </summary>
        protected IReadOnlyDictionary<string, CannoliRouteId> ReturnRoutes => State.ReturnRoutes;

        private readonly SocketInteraction? _interaction;

        private bool _isBuilt;

        private const string NextPageRouteName = "CannoliKit.NextPageRoute";
        private const string PreviousPageRouteName = "CannoliKit.PreviousPageRoute";
        private const string DefaultCancelRouteName = "CannoliKit.DefaultCancelRoute";

        /// <summary>
        /// Initializes a new instance of <see cref="CannoliModule{TContext,TState}"/>. This constructor is intended for use with Dependency Injection.
        /// </summary>
        protected CannoliModule(
            TContext db,
            DiscordSocketClient discordClient,
            CannoliModuleFactoryConfiguration factoryConfiguration)
        {
            Db = db;
            DiscordClient = discordClient;
            Pagination = new Pagination.Pagination();
            Alerts = new Alerts.AlertsSettings();
            User = factoryConfiguration.RequestingUser;

            State = new TState
            {
                Db = Db
            };

            RouteManager = new RouteManager(Db, GetType(), State);
            Cancellation = new Cancellation.CancellationSettings(State);

            _interaction = factoryConfiguration.Interaction;

            if (factoryConfiguration.Routing?.CancellationRouteId != null)
            {
                Cancellation.SetRoute(factoryConfiguration.Routing.CancellationRouteId);
            }

            if (factoryConfiguration.Routing?.ReturnRouteIds != null)
            {
                foreach (var r in factoryConfiguration.Routing.ReturnRouteIds)
                {
                    r.Value.Route!.StateIdToBeDeleted = State.Id;
                    State.ReturnRoutes.Add(r.Key, r.Value);
                }
            }
        }

        /// <inheritdoc/>
        public override async Task<CannoliModuleComponents> BuildComponentsAsync()
        {
            if (_isBuilt)
            {
                throw new InvalidOperationException(
                    "Module has already been built. It cannot be rebuilt.");
            }

            var renderParts = await BuildLayout();

            await RouteUtility.RemoveRoutes(Db, State.Id);

            var content = renderParts.Text;

            var componentBuilder = renderParts.ComponentBuilder
                ?? new ComponentBuilder();

            await AddPaginationButtons(componentBuilder);

            await AddCancellationButton(componentBuilder);

            var embeds = new List<Embed>();

            if (Alerts.InfoMessage != null)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Description = $"{Alerts.InfoMessage}",
                    Color = Alerts.InfoMessage.Color
                };

                embeds.Add(embedBuilder.Build());
            }

            if (Alerts.ErrorMessage != null)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Description = $"{Alerts.ErrorMessage}",
                    Color = Alerts.ErrorMessage.Color
                };

                embeds.Add(embedBuilder.Build());
            }

            if (renderParts.EmbedBuilder != null)
            {
                embeds.Add(renderParts.EmbedBuilder.Build());
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                content = null;
            }

            if (componentBuilder.ActionRows == null || componentBuilder.ActionRows.Count == 0)
            {
                componentBuilder = null;
            }

            if (embeds.Count == 0)
            {
                embeds = null;
            }

            await SaveModuleState();

            _isBuilt = true;

            return new CannoliModuleComponents(
                content,
                embeds?.ToArray(),
                componentBuilder?.Build());
        }

        /// <summary>
        /// Builds the Cannoli Module layout which is used to render the module in Discord.
        /// </summary>
        /// <returns>The layout to be used to generate the module in Discord.</returns>
        protected abstract Task<CannoliModuleLayout> BuildLayout();

        /// <summary>
        /// Modifies the Discord response with a refreshed module.
        /// </summary>
        protected async Task RefreshModuleAsync()
        {
            if (_interaction == null)
            {
                throw new InvalidOperationException(
                    "Module does not have an interaction context to reply to.");
            }

            if (_interaction.HasResponded == false)
            {
                await _interaction.DeferAsync();
            }

            await _interaction.ModifyOriginalResponseAsync(this);
        }

        internal override async Task SaveModuleState()
        {
            if (State.IsExpiringNow) return;
            if (State.IsSaved) return;
            await State.Save();
            RouteManager.AddRoutes();
        }

        internal override async Task LoadModuleState(CannoliRoute route)
        {
            if (route.StateIdToBeDeleted != null)
            {
                await SaveStateUtility.RemoveStateAsync(Db, route.StateIdToBeDeleted);
            }

            var state = await SaveStateUtility.GetState<TState>(
                Db,
                route.StateId) ?? throw new ModuleStateNotFoundException(
                    $"Unable to find Cannoli Module State with ID {route.StateId}.");

            state.Db = Db;

            State = state;
            Cancellation.State = state;
            RouteManager.State = state;

            await LoadReturnRoutes();
        }

        internal async Task OnModulePageChanged(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            var offset = int.Parse(route.Parameter1!);

            Pagination.PageNumber += offset;

            await RefreshModuleAsync();
        }

        internal async Task OnModuleCancelled(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            await State.ExpireNowAsync();
            await messageComponent.DeleteOriginalResponseAsync();
        }

        private async Task AddPaginationButtons(ComponentBuilder componentBuilder)
        {
            if (Pagination.IsEnabled == false || Pagination.NumPages <= 1) return;

            var rowBuilder = new ActionRowBuilder();

            rowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = await RouteManager.CreateMessageComponentRouteAsync(
                    callback: OnModulePageChanged,
                    routeName: PreviousPageRouteName,
                    parameter1: (Pagination.PageNumber - 1).ToString()),
                Emote = Pagination.PreviousArrowEmoji,
                Style = ButtonStyle.Secondary,
            });

            rowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = await RouteManager.CreateMessageComponentRouteAsync(
                    callback: OnModulePageChanged,
                    routeName: NextPageRouteName,
                    parameter1: (Pagination.PageNumber + 1).ToString()),
                Emote = Pagination.NextArrowEmoji,
                Style = ButtonStyle.Secondary,
            });

            componentBuilder.ActionRows ??= [];
            componentBuilder.ActionRows.Insert(0, rowBuilder);
        }

        private async Task AddCancellationButton(ComponentBuilder componentBuilder)
        {
            if (Cancellation.IsEnabled == false) return;

            componentBuilder.ActionRows ??= [];

            var lastRowBuilder = componentBuilder.ActionRows.LastOrDefault();

            var rowBuilder = new ActionRowBuilder();

            if (lastRowBuilder != null && lastRowBuilder.Components.Count == 1 && lastRowBuilder.Components[0] is ButtonComponent)
            {
                rowBuilder = lastRowBuilder;
            }
            else
            {
                componentBuilder.ActionRows.Add(rowBuilder);
            }

            var cancellationRoute = Cancellation.HasCustomRouting
                ? Cancellation.Route!
                : await RouteManager.CreateMessageComponentRouteAsync(
                    routeName: DefaultCancelRouteName,
                    callback: OnModuleCancelled);

            cancellationRoute.Route!.StateIdToBeDeleted = State.Id;

            rowBuilder.Components.Insert(
                0,
                new ButtonBuilder
                {
                    CustomId = cancellationRoute,
                    Label = Cancellation.ButtonLabel,
                    Style = ButtonStyle.Secondary,
                }.Build());
        }

        private async Task LoadReturnRoutes()
        {
            if (State.CancelRoute is { Route: null })
            {
                var route = await Db.CannoliRoutes
                    .FirstOrDefaultAsync(x => x.Id == State.CancelRoute.RouteId);

                if (route == null)
                {
                    State.CancelRoute = null;
                }
                else
                {
                    State.CancelRoute.Route = route;
                }
            }

            var routeFragments = State.ReturnRoutes.Values
                .Where(x => x.Route == null)
                .ToList();

            var routeIds = routeFragments
                .Select(x => x.RouteId)
                .ToList();

            var routes = await Db.CannoliRoutes
                .Where(x => routeIds.Contains(x.Id))
                .ToListAsync();

            foreach (var kv in State.ReturnRoutes.ToList())
            {
                var route = routes.FirstOrDefault(x => x.Id == kv.Value.RouteId);

                if (route == null)
                {
                    State.ReturnRoutes.Remove(kv.Key);
                }
                else
                {
                    State.ReturnRoutes[kv.Key].Route = route;
                }
            }
        }
    }
}
