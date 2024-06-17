using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Modules;
using CannoliKit.Modules.Pagination;
using CannoliKit.Modules.Routing;
using Demo.Models;
using Demo.Modules.Menu;
using Demo.Processors.GroceryOrder;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Demo.Modules.Cart
{
    internal class CartModule : CannoliModule<DemoDbContext, CartState>
    {
        private readonly ICannoliModuleFactory _cannoliModuleFactory;
        private readonly ICannoliJobQueue<GroceryOrderJob> _groceryOrderJobQueue;

        public CartModule(
            DemoDbContext db,
            DiscordSocketClient discordClient,
            CannoliModuleFactoryConfiguration factoryConfiguration,
            ICannoliModuleFactory cannoliModuleFactory,
            ICannoliJobQueue<GroceryOrderJob> groceryOrderJobQueue)
            : base(db, discordClient, factoryConfiguration)
        {
            _cannoliModuleFactory = cannoliModuleFactory;
            _groceryOrderJobQueue = groceryOrderJobQueue;

            Cancellation.IsEnabled = true;
            Cancellation.ButtonLabel = "Cancel Order";

            State.ExpiresOn = DateTime.UtcNow.AddDays(3);
        }

        protected override async Task<CannoliModuleLayout> BuildLayout()
        {
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
                    FoodItem = g.Select(x => x).First(),
                    Count = g.Count()
                })
                .ToList();

            var paginationResult = Pagination.Setup(
                items: groupedItems,
                formatter: x => $"{x.Marker} {x.Item.FoodItem.Emoji} `{x.Item.FoodItem.Name.PadRight(x.MaxLengthOf(y => y.FoodItem.Name))}` `x{x.Item.Count}`",
                listType: ListType.Bullet);

            var fields = new List<EmbedFieldBuilder>();
            fields.AddRange(paginationResult.Fields);

            if (fields.Count == 0)
            {
                Alerts.SetInfoMessage(
                    "Use the buttons below to add items to your cart.");

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
                    CustomId = await RouteManager.CreateMessageComponentRouteAsync(
                        callback: OnOpenMenu)
                }
            };

            if (State.Items.Count > 0)
            {
                buttons.Add(new ButtonBuilder
                {
                    Label = "Checkout",
                    Style = ButtonStyle.Success,
                    CustomId = await RouteManager.CreateMessageComponentRouteAsync(
                        callback: OnCheckout)
                });

                buttons.Add(new ButtonBuilder
                {
                    Label = "Reset Cart",
                    Style = ButtonStyle.Danger,
                    CustomId = await RouteManager.CreateMessageComponentRouteAsync(
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

            return new CannoliModuleLayout
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
            var groceryOrder = new GroceryOrder
            {
                Items = State.Items.Select(x => new GroceryOrderItem { FoodItemId = x }).ToList(),
                OrderedOn = DateTime.UtcNow,
                UserId = User.Id.ToString()
            };

            Db.GroceryOrders.Add(groceryOrder);

            await Db.SaveChangesAsync();

            _groceryOrderJobQueue.EnqueueJob(new GroceryOrderJob
            {
                OrderId = groceryOrder.Id
            });

            await messageComponent.ModifyOriginalResponseAsync(x =>
            {
                x.Embeds = null;
                x.Components = null;
                x.Content =
                    $"Ok, your order is placed and will be fulfilled shortly! For reference your Order ID is `{groceryOrder.Id}`.";
            });

            await State.ExpireNow();
        }

        private async Task OnOpenMenu(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            var builder = new RouteConfigurationBuilder();

            var menuModule = _cannoliModuleFactory.CreateModule<MenuModule>(
                requestingUser: User,
                routing: builder
                    .WithCancellationRoute(
                        routeId: await RouteManager.CreateMessageComponentRouteAsync(
                            callback: OnRefreshCart))
                    .WithReturnRoute(
                        tag: "ItemSelected",
                        routeId: await RouteManager.CreateMessageComponentRouteAsync(
                            callback: OnItemSelected))
                    .Build());

            await messageComponent.ModifyOriginalResponseAsync(menuModule);
        }

        private async Task OnItemSelected(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            var foodItemId = int.Parse(messageComponent.Data.Values.ElementAt(0));

            var foodItem = Db.FoodItems.First(x => x.Id == foodItemId);

            State.Items.Add(foodItem.Id);

            await RefreshModuleAsync();
        }

        private async Task OnRefreshCart(SocketMessageComponent messagecomponent, CannoliRoute route)
        {
            await RefreshModuleAsync();
        }
    }
}
