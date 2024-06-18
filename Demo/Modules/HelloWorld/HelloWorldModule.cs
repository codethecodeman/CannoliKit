using CannoliKit.Models;
using CannoliKit.Modules;
using Discord;
using Discord.WebSocket;

namespace Demo.Modules.HelloWorld
{
    internal class HelloWorldModule : CannoliModule<DemoDbContext, HelloWorldState>
    {
        public HelloWorldModule(
            DemoDbContext db,
            DiscordSocketClient discordClient,
            CannoliModuleFactoryConfiguration factoryConfiguration)
            : base(db, discordClient, factoryConfiguration) { }

        protected override async Task<CannoliModuleLayout> BuildLayout()
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = "Hello world!",
                Timestamp = DateTimeOffset.UtcNow
            };

            if (State.LastHelloOn != null)
            {
                embedBuilder.Description = $"Last Hello received on {State.LastHelloOn.Value}";
            }

            var componentBuilder = new ComponentBuilder()
                .WithButton(
                    label: "Hello",
                    customId: await RouteManager.CreateMessageComponentRouteAsync(
                        callback: OnHello));

            return new CannoliModuleLayout
            {
                EmbedBuilder = embedBuilder,
                ComponentBuilder = componentBuilder
            };
        }

        private async Task OnHello(SocketMessageComponent messageComponent, CannoliRoute route)
        {
            State.LastHelloOn = DateTime.UtcNow;
            await RefreshModuleAsync();
        }
    }
}
