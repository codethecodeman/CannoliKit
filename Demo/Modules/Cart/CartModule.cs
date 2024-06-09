using CannoliKit.Interfaces;
using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
using Discord;
using Discord.WebSocket;

namespace Demo.Modules.Cart
{
    internal class CartModule : CannoliModule<DemoDbContext, CartState>
    {
        private readonly ICannoliModuleFactory _cannoliModuleFactory;

        public CartModule(
            DemoDbContext db,
            DiscordSocketClient discordClient,
            RouteConfiguration? routeConfiguration,
            ICannoliModuleFactory cannoliModuleFactory)
            : base(db, discordClient, routeConfiguration)
        {
            _cannoliModuleFactory = cannoliModuleFactory;
        }

        protected override async Task<CannoliModuleParts> SetupModule()
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = "My Order",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Cart ID {State.CartId}"
                },
                Timestamp = DateTimeOffset.Now
            };

            return new CannoliModuleParts
            {
                EmbedBuilder = embedBuilder
            };
        }
    }
}
