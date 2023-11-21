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
    public abstract class CannoliModule<TContext, TState>
        where TContext : DbContext, ICannoliDbContext
        where TState : CannoliModuleState, new()
    {
        protected readonly DiscordSocketClient DiscordClient;
        protected readonly TContext Db;
        protected readonly Pagination.Pagination Pagination;
        protected readonly RouteManager RouteManager;
        protected readonly Cancellation.CancellationSettings Cancellation;
        protected TState State { get; private set; }

        public string? CancelRouteId
        {
            get => State.CancelRouteId;
            set => State.CancelRouteId = value;
        }

        protected CannoliModule(TContext db, DiscordSocketClient discordClient)
        {
            Db = db;
            DiscordClient = discordClient;
            State = new TState();
            Pagination = new Pagination.Pagination();
            RouteManager = new RouteManager(Db, GetType(), State);
            Cancellation = new Cancellation.CancellationSettings(State);

            WireupState();
        }

        public async Task<CannoliModuleComponents> BuildComponents()
        {
            var scaffolding = await Setup();

            var content = scaffolding.Content;

            var componentBuilder = scaffolding.ComponentBuilder
                ?? new ComponentBuilder();

            AppendPaginationButtons(componentBuilder);

            AppendCancellationButton(componentBuilder);

            await UpdateCancellationRoute();

            await FinalizeRoutes();

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

            if (scaffolding.EmbedBuilder != null)
            {
                embeds.Add(scaffolding.EmbedBuilder.Build());
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

            return new CannoliModuleComponents(
                content,
                embeds?.ToArray(),
                componentBuilder?.Build());
        }

        protected async Task LoadModuleState(string stateId)
        {
            var state = await SaveStateUtility.GetState<TState>(
                Db,
                stateId);

            if (state == null) return;

            state.ErrorMessage = null;

            State = state;
            Cancellation.State = state;
            RouteManager.State = state;

            WireupState();
        }

        protected abstract Task<CannoliModuleScaffolding> Setup();

        protected async Task OnPageChanged(SocketMessageComponent messageComponent, Route route)
        {
            var offset = int.Parse(route.Parameter1!);

            Pagination.PageNumber += offset;

            await messageComponent.ModifyOriginalResponseAsync(this);
        }

        protected async Task OnExit(SocketMessageComponent messageComponent, Route route)
        {
            await messageComponent.DeleteOriginalResponseAsync();

            if (Cancellation.DoesDeleteCurrentState)
            {
                await State.Delete();
            }
        }

        private void AppendPaginationButtons(ComponentBuilder componentBuilder)
        {
            if (Pagination.IsEnabled == false || Pagination.NumPages <= 1) return;

            var rowBuilder = new ActionRowBuilder();

            rowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = RouteManager.CreateMessageComponentRoute(
                    callback: OnPageChanged,
                    parameter1: (Pagination.PageNumber - 1).ToString()),
                Emote = Emoji.Parse("⬅️"),
                Style = ButtonStyle.Secondary,
            });

            rowBuilder.WithButton(new ButtonBuilder()
            {
                CustomId = RouteManager.CreateMessageComponentRoute(
                    callback: OnPageChanged,
                    parameter1: (Pagination.PageNumber + 1).ToString()),
                Emote = Emoji.Parse("➡️"),
                Style = ButtonStyle.Secondary,
            });

            componentBuilder.ActionRows ??= new List<ActionRowBuilder>();
            componentBuilder.ActionRows.Insert(0, rowBuilder);
        }

        private void AppendCancellationButton(ComponentBuilder componentBuilder)
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

            rowBuilder.Components.Insert(0, new ButtonBuilder()
            {
                CustomId =
                    Cancellation.CustomRouteId
                    ?? RouteManager.CreateMessageComponentRoute(callback: OnExit),
                Label = Cancellation.ButtonLabel,
                Style = ButtonStyle.Secondary,
            }.Build());

        }

        private async Task UpdateCancellationRoute()
        {
            if (Cancellation.IsEnabled == false) return;

            if (Cancellation is { CustomRouteId: not null, DoesDeleteCurrentState: true })
            {
                var route = await RouteUtility.GetRoute(Db, Cancellation.CustomRouteId);

                if (route != null)
                {
                    route.StateIdToBeDeleted = State.Id;
                }
            }
        }

        private async Task FinalizeRoutes()
        {
            if (Cancellation.IsEnabled || Pagination.IsEnabled)
            {
                if (State.DidSaveAtLeastOnce == false)
                {
                    throw new InvalidOperationException(
                        "Cancellation and Pagination requires a module save state for routing. The state must be saved at least once.");
                }

                await State.Save();
            }
        }

        private void WireupState()
        {
            State.Db = Db;
            State.OnSave += (s, e) =>
            {
                RouteManager.AddRoutes();
            };
        }
    }
}
