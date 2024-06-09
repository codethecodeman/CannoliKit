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
    public abstract class CannoliModule<TContext, TState> : CannoliModuleBase
        where TContext : DbContext, ICannoliDbContext
        where TState : CannoliModuleState, new()
    {
        protected readonly DiscordSocketClient DiscordClient;
        protected readonly TContext Db;
        protected readonly Pagination.Pagination Pagination;
        protected readonly RouteManager RouteManager;
        protected readonly Cancellation.CancellationSettings Cancellation;
        protected readonly SocketUser User;
        protected TState State { get; private set; }
        protected IReadOnlyDictionary<string, CannoliRouteId> ReturnRoutes => State.ReturnRoutes;
        private const string DefaultCancelRouteName = "CannoliKit.DefaultCancelRoute";

        protected CannoliModule(
            TContext db,
            DiscordSocketClient discordClient,
            CannoliModuleConfiguration configuration)
        {
            Db = db;
            DiscordClient = discordClient;
            Pagination = new Pagination.Pagination();
            User = configuration.RequestingUser;

            State = new TState
            {
                Db = Db
            };

            RouteManager = new RouteManager(Db, GetType(), State);
            Cancellation = new Cancellation.CancellationSettings(State);

            if (configuration.Routing?.CancellationRouteId != null)
            {
                Cancellation.SetRoute(configuration.Routing.CancellationRouteId);
            }

            if (configuration.Routing?.ReturnRouteIds != null)
            {
                foreach (var r in configuration.Routing.ReturnRouteIds)
                {
                    r.Value.Route!.StateIdToBeDeleted = State.Id;
                    State.ReturnRoutes.Add(r.Key, r.Value);
                }
            }
        }

        internal override async Task<CannoliModuleComponents> BuildComponents()
        {
            var renderParts = await SetupModule();

            await RouteUtility.RemoveRoutes(Db, State.Id);

            var content = renderParts.Content;

            var componentBuilder = renderParts.ComponentBuilder
                ?? new ComponentBuilder();

            await AddPaginationButtons(componentBuilder);

            await AddCancellationButton(componentBuilder);

            var embeds = new List<Embed>();

            if (string.IsNullOrWhiteSpace(State.InfoMessage) == false)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Description = $"ℹ️ {State.InfoMessage}",
                    Color = new Color(88, 101, 242)
                };

                embeds.Add(embedBuilder.Build());
            }

            if (string.IsNullOrWhiteSpace(State.ErrorMessage) == false)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Description = $"❌ {State.ErrorMessage}",
                    Color = Color.Red,
                };

                embeds.Add(embedBuilder.Build());

                State.ErrorMessage = null;
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

            return new CannoliModuleComponents(
                content,
                embeds?.ToArray(),
                componentBuilder?.Build());
        }

        protected abstract Task<CannoliModuleParts> SetupModule();

        protected async Task UpdateModule(SocketMessageComponent messageComponent)
        {
            await messageComponent.ModifyOriginalResponseAsync(this);
        }

        protected async Task UpdateModule(SocketModal modal)
        {
            await modal.ModifyOriginalResponseAsync(this);
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
                await SaveStateUtility.RemoveState(Db, route.StateIdToBeDeleted);
            }

            var state = await SaveStateUtility.GetState<TState>(
                Db,
                route.StateId);

            if (state == null)
            {
                throw new ModuleStateNotFoundException(
                    $"Unable to find module state {route.StateId}");
            }

            state.Db = Db;

            state.InfoMessage = null;
            state.ErrorMessage = null;

            State = state;
            Cancellation.State = state;
            RouteManager.State = state;

            await LoadReturnRoutes();
        }

        internal async Task OnModulePageChanged(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            var offset = int.Parse(route.Parameter1!);

            Pagination.PageNumber += offset;

            await messageComponent.ModifyOriginalResponseAsync(this);
        }

        internal async Task OnModuleCancelled(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            await State.ExpireNow();
            await messageComponent.DeleteOriginalResponseAsync();
        }

        private async Task AddPaginationButtons(ComponentBuilder componentBuilder)
        {
            if (Pagination.IsEnabled == false || Pagination.NumPages <= 1) return;

            var rowBuilder = new ActionRowBuilder();

            rowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = await RouteManager.CreateMessageComponentRoute(
                    callback: OnModulePageChanged,
                    parameter1: (Pagination.PageNumber - 1).ToString()),
                Emote = Emoji.Parse("⬅️"),
                Style = ButtonStyle.Secondary,
            });

            rowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = await RouteManager.CreateMessageComponentRoute(
                    callback: OnModulePageChanged,
                    parameter1: (Pagination.PageNumber + 1).ToString()),
                Emote = Emoji.Parse("➡️"),
                Style = ButtonStyle.Secondary,
            });

            componentBuilder.ActionRows ??= new List<ActionRowBuilder>();
            componentBuilder.ActionRows.Insert(0, rowBuilder);
        }

        private async Task AddCancellationButton(ComponentBuilder componentBuilder)
        {
            if (Cancellation.IsEnabled == false) return;

            componentBuilder.ActionRows ??= new List<ActionRowBuilder>();

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
                : await RouteManager.CreateMessageComponentRoute(
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
