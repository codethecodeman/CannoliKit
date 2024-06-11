using CannoliKit.Modules;
using CannoliKit.Modules.Pagination;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Demo.Modules.Menu
{
    internal class MenuModule : CannoliModule<DemoDbContext, MenuState>
    {
        public MenuModule(
            DemoDbContext db,
            DiscordSocketClient discordClient,
            CannoliModuleFactoryConfiguration factoryConfiguration)
            : base(db, discordClient, factoryConfiguration)
        {
            Pagination.IsEnabled = true;
            Pagination.NumItemsPerRow = 2;
            Pagination.NumItemsPerField = 10;
            Pagination.NumItemsPerPage = 10;

            Cancellation.IsEnabled = true;
            Cancellation.ButtonLabel = "Back To Cart";
        }

        protected override async Task<CannoliModuleParts> SetupModule()
        {
            var foodItems = await Db.FoodItems
                .OrderBy(x => x.Name)
                .ToListAsync();

            Pagination.SetItemCount(foodItems.Count);

            var pagedFoodItems = Pagination.GetListItems(
                items: foodItems,
                listType: ListType.Number,
                resetListCounterBetweenPages: true);

            var length = pagedFoodItems.Max(x => x.Item.Name.Length) + 1;

            var fields = Pagination.GetEmbedFieldBuilders(
                pagedFoodItems
                    .Select(x => $"`{x.Marker}` {x.Item.Emoji} `{x.Item.Name.PadRight(length)}` ")
                    .ToList());

            var menuBuilder = new SelectMenuBuilder
            {
                CustomId = ReturnRoutes["ItemSelected"]
            };

            foreach (var item in pagedFoodItems)
            {
                menuBuilder.AddOption(new SelectMenuOptionBuilder
                {
                    Emote = new Emoji(item.Item.Emoji),
                    Label = $"{item.Marker} {item.Item.Name}",
                    Value = item.Item.Id.ToString()
                });
            }

            var componentBuilder = new ComponentBuilder();
            var row1 = new ActionRowBuilder();
            row1.AddComponent(menuBuilder.Build());
            componentBuilder.AddRow(row1);

            State.InfoMessage = "Select an item from the menu to add it to your cart.";

            var embedBuilder = new EmbedBuilder
            {
                Title = "Menu",
                Fields = fields,
                Timestamp = DateTimeOffset.UtcNow
            };

            return new CannoliModuleParts
            {
                EmbedBuilder = embedBuilder,
                ComponentBuilder = componentBuilder,
            };
        }
    }
}
