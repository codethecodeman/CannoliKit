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

        private ActionRowBuilder? _pageButtonsActionRowBuilder, _pageSelectActionRowBuilder;

        private bool _isBuilt;

        private const int PageNumberControlBreakpoint = 5;
        private const string NextPageRouteName = "CannoliKit.NextPageRoute";
        private const string PreviousPageRouteName = "CannoliKit.PreviousPageRoute";
        private const string SelectPageRouteName = "CannoliKit.SelectPageRoute";
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
            Alerts = new Alerts.AlertsSettings();
            User = factoryConfiguration.RequestingUser;

            State = new TState
            {
                Db = Db
            };

            RouteManager = new RouteManager(Db, GetType(), State);
            Cancellation = new Cancellation.CancellationSettings(State);
            Pagination = new Pagination.Pagination(State);

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

            await AddPaginationComponents(componentBuilder);

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

            if (componentBuilder.ActionRows?.Count > 5 && _pageButtonsActionRowBuilder != null)
            {
                if (Pagination.NumPages > PageNumberControlBreakpoint)
                {
                    componentBuilder.ActionRows.Remove(_pageButtonsActionRowBuilder);
                }
                else if (_pageSelectActionRowBuilder != null)
                {
                    // This case should ideally not be met.
                    componentBuilder.ActionRows.Remove(_pageSelectActionRowBuilder);
                }
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
        /// <param name="allowedMentions">Allowed mentions.</param>
        protected async Task RefreshModuleAsync(AllowedMentions? allowedMentions = null)
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

            await _interaction.ModifyOriginalResponseAsync(this, allowedMentions);
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

            await LoadModuleState(route.StateId);
        }

        internal override async Task LoadModuleState(string stateId, bool useCustomStateId = false)
        {
            TState? state = null;

            try
            {
                state = await SaveStateUtility.GetState<TState>(Db, stateId);
            }
            catch (Exception)
            {
                if (useCustomStateId == false) throw;
            }

            switch (state)
            {
                case null when useCustomStateId == false:
                    throw new ModuleStateNotFoundException(
                        $"Unable to find Cannoli Module State with ID {stateId}.");
                case null when useCustomStateId:
                    State.Id = stateId;
                    return;
            }

            state!.Db = Db;

            State = state;
            Cancellation.State = state;
            RouteManager.State = state;
            Pagination.State = state;

            await LoadReturnRoutes();
        }

        internal async Task OnModulePageChanged(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            var offset = int.Parse(route.Parameter1!);
            var id = route.Parameter2!;

            if (offset == 0)
            {
                var selectedPageNumber = int.Parse(messageComponent.Data.Values.First());
                State.PageNumbers[id] = selectedPageNumber;
            }
            else
            {
                State.PageNumbers[id] += offset;
            }

            await RefreshModuleAsync(AllowedMentions.None);
        }

        internal async Task OnModuleCancelled(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            await State.ExpireNowAsync();
            await messageComponent.DeleteOriginalResponseAsync();
        }

        private async Task AddPaginationComponents(ComponentBuilder componentBuilder)
        {
            if (Pagination.IsEnabled == false || Pagination.NumPages <= 1) return;

            _pageButtonsActionRowBuilder = new ActionRowBuilder();

            _pageButtonsActionRowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = await RouteManager.CreateMessageComponentRouteAsync(
                    callback: OnModulePageChanged,
                    routeName: PreviousPageRouteName,
                    parameter1: "-1",
                    parameter2: Pagination.PaginationId),
                Emote = Pagination.PreviousArrowEmoji,
                Style = ButtonStyle.Secondary,
            });

            _pageButtonsActionRowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = await RouteManager.CreateMessageComponentRouteAsync(
                    callback: OnModulePageChanged,
                    routeName: NextPageRouteName,
                    parameter1: "1",
                    parameter2: Pagination.PaginationId),
                Emote = Pagination.NextArrowEmoji,
                Style = ButtonStyle.Secondary,
            });

            componentBuilder.ActionRows ??= [];
            componentBuilder.ActionRows.Insert(0, _pageButtonsActionRowBuilder);

            if (Pagination.NumPages > PageNumberControlBreakpoint)
            {
                var totalPages = Pagination.NumPages;
                var currentPage = Pagination.PageNumber;
                var maxItems = 25;
                var halfWindow = maxItems / 2;

                // Calculate the starting index so that the current page is centered.
                var startIndex = currentPage - halfWindow;
                if (startIndex < 0) startIndex = 0;

                // Calculate the ending index.
                var endIndex = startIndex + maxItems - 1;
                if (endIndex >= totalPages)
                {
                    endIndex = totalPages - 1;
                    // If we hit the end, readjust the start index.
                    startIndex = Math.Max(0, endIndex - maxItems + 1);
                }

                // Number of items in the select menu.
                var count = endIndex - startIndex + 1;

                _pageSelectActionRowBuilder = new ActionRowBuilder();
                _pageSelectActionRowBuilder.WithSelectMenu(new SelectMenuBuilder()
                {
                    CustomId = await RouteManager.CreateMessageComponentRouteAsync(
                        callback: OnModulePageChanged,
                        routeName: SelectPageRouteName,
                        parameter1: "0",
                        parameter2: Pagination.PaginationId),
                    Options = Enumerable.Range(startIndex, count)
                        .Select(x => new SelectMenuOptionBuilder
                        {
                            Label = $"Page {x + 1}",
                            Value = x.ToString(),
                            IsDefault = Pagination.PageNumber == x
                        })
                        .ToList()
                });

                componentBuilder.ActionRows.Insert(1, _pageSelectActionRowBuilder);
            }
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
                });
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
