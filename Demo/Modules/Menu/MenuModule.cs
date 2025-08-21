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
            Cancellation.IsEnabled = true;
            Cancellation.ButtonLabel = "Back To Cart";
        }

        protected override async Task<CannoliModuleLayout> BuildLayout()
        {
            var foodItems = await Db.FoodItems
                .OrderBy(x => x.Name)
                .ToListAsync();

            var paginationResult = Pagination.Setup(
                items: foodItems,
                formatter: x => $"`{x.Marker}` {x.Item.Emoji} `{x.Item.Name.PadRight(x.MaxLengthOf(y => y.Name))}` ",
                listType: ListType.Number,
                numItemsPerRow: 2,
                numItemsPerPage: 10,
                numItemsPerField: 10);

            var menuBuilder = new SelectMenuBuilder
            {
                CustomId = ReturnRoutes["ItemSelected"]
            };

            foreach (var item in paginationResult.Items)
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
            row1.AddComponent(menuBuilder);
            componentBuilder.AddRow(row1);

            Alerts.SetInfoMessage(
                content: "Select an item from the menu to add it to your cart.");

            var fields = new List<EmbedFieldBuilder>();
            fields.AddRange(paginationResult.Fields);

            var embedBuilder = new EmbedBuilder
            {
                Title = "Food Menu",
                Fields = fields,
                Timestamp = DateTimeOffset.UtcNow
            };

            return new CannoliModuleLayout
            {
                EmbedBuilder = embedBuilder,
                ComponentBuilder = componentBuilder,
            };
        }
    }
}
