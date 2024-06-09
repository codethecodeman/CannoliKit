﻿using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Modules;
using CannoliKit.Modules.Pagination;
using CannoliKit.Modules.Routing;
using Demo.Models;
using Demo.Modules.Menu;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Demo.Modules.Cart
{
    internal class CartModule : CannoliModule<DemoDbContext, CartState>
    {
        private readonly ICannoliModuleFactory _cannoliModuleFactory;

        public CartModule(
            DemoDbContext db,
            DiscordSocketClient discordClient,
            CannoliModuleConfiguration configuration,
            ICannoliModuleFactory cannoliModuleFactory)
            : base(db, discordClient, configuration)
        {
            _cannoliModuleFactory = cannoliModuleFactory;

            Pagination.IsEnabled = true;
            Pagination.NumItemsPerRow = 1;
            Pagination.NumItemsPerField = 10;
            Pagination.NumItemsPerPage = 10;

            Cancellation.IsEnabled = true;
            Cancellation.ButtonLabel = "Cancel Order";
        }

        protected override async Task<CannoliModuleParts> SetupModule()
        {
            Pagination.SetItemCount(State.Items.Count);

            var foodItems = await Db.FoodItems.ToListAsync();

            var foodItemsInCart = new List<FoodItem>();

            foreach (var itemId in State.Items)
            {
                var foodItem = foodItems.FirstOrDefault(x => x.Id == itemId);

                if (foodItem != null)
                {
                    foodItemsInCart.Add(foodItem);
                }
                else
                {
                    State.Items.RemoveAll(x => x == itemId);
                }
            }

            var groupedItems = foodItemsInCart
                .OrderBy(x => x.Name)
                .GroupBy(x => x.Id)
                .Select(g => new
                {
                    Id = g.Key,
                    Item = g.Select(x => x).First(),
                    Count = g.Count()
                })
                .ToList();

            var pagedItems = Pagination.GetListItems(groupedItems, ListType.Bullet);

            var fields = Pagination.GetEmbedFieldBuilders(
                pagedItems
                    .Select(x => $"{x.Marker} {x.Item.Item.Emoji} {x.Item.Item.Name} (x{x.Item.Count})")
                    .ToList());

            if (fields.Count == 0)
            {
                State.InfoMessage = "Use the buttons below to add items to your cart.";

                fields.Add(new EmbedFieldBuilder
                {
                    Name = "There are no items in your cart",
                    Value = "​\u200b"
                });
            }

            var buttons = new List<ButtonBuilder>
            {
                new()
                {
                    Label = "Open Menu",
                    Style = ButtonStyle.Primary,
                    CustomId = await RouteManager.CreateMessageComponentRoute(
                        callback: OnOpenMenu)
                }
            };

            if (State.Items.Count > 0)
            {
                buttons.Add(new ButtonBuilder
                {
                    Label = "Checkout",
                    Style = ButtonStyle.Success,
                    CustomId = await RouteManager.CreateMessageComponentRoute(
                        callback: OnCheckout)
                });

                buttons.Add(new ButtonBuilder
                {
                    Label = "Reset Cart",
                    Style = ButtonStyle.Danger,
                    CustomId = await RouteManager.CreateMessageComponentRoute(
                        callback: OnResetCart)
                });
            }

            var embedBuilder = new EmbedBuilder
            {
                Title = $"{User.GlobalName}'s Grocery Order",
                Fields = fields,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Cart ID {State.CartId}"
                },
                Timestamp = DateTimeOffset.Now
            };

            var componentBuilder = new ComponentBuilder();

            var buttonRow = new ActionRowBuilder();

            foreach (var button in buttons)
            {
                buttonRow.AddComponent(button.Build());
            }

            componentBuilder.AddRow(buttonRow);

            await Task.CompletedTask;

            return new CannoliModuleParts
            {
                EmbedBuilder = embedBuilder,
                ComponentBuilder = componentBuilder,
            };
        }

        private async Task OnResetCart(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            State.Items.Clear();
            await messageComponent.ModifyOriginalResponseAsync(this);
        }

        private async Task OnCheckout(SocketMessageComponent messageComponent, CannoliRoute route)
        {

        }

        private async Task OnOpenMenu(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            var builder = new RouteConfigurationBuilder();

            var menuModule = _cannoliModuleFactory.CreateModule<MenuModule>(
                requestingUser: User,
                routing: builder
                    .WithCancellationRoute(
                        routeId: await RouteManager.CreateMessageComponentRoute(
                            callback: OnRefreshCart))
                    .WithReturnRoute(
                        tag: "ItemSelected",
                        routeId: await RouteManager.CreateMessageComponentRoute(
                            callback: OnItemSelected))
                    .Build());

            await messageComponent.ModifyOriginalResponseAsync(menuModule);
        }

        private async Task OnItemSelected(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            var foodItemId = int.Parse(messageComponent.Data.Values.ElementAt(0));

            var foodItem = Db.FoodItems.First(x => x.Id == foodItemId);

            State.Items.Add(foodItem.Id);

            await messageComponent.ModifyOriginalResponseAsync(this);
        }

        private async Task OnRefreshCart(SocketMessageComponent messagecomponent, CannoliRoute route)
        {
            await messagecomponent.ModifyOriginalResponseAsync(this);
        }
    }
}
