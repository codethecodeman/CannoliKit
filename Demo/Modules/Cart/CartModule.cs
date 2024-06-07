using CannoliKit.Modules;
using CannoliKit.Modules.Routing;
using Discord.WebSocket;

namespace Demo.Modules.Cart
{
    internal class CartModule : CannoliModule<DemoDbContext, CartState>
    {
        public CartModule(
            DemoDbContext db,
            DiscordSocketClient discordClient,
            RouteConfiguration? routeConfiguration)
            : base(db, discordClient, routeConfiguration)
        {
        }

        protected override Task<CannoliModuleParts> SetupModule()
        {
            throw new NotImplementedException();
        }
    }
}
