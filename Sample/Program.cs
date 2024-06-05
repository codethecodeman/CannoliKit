using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sample
{
    public class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            var appPath = AppContext.BaseDirectory;

            var collection = new ServiceCollection();

            collection.AddSingleton(new DiscordSocketConfig()
            {
                //...
            });

            collection.AddSingleton(new DiscordSocketClient());

            collection.AddDbContext<SampleDbContext>(opt =>
                opt.UseSqlite(Path.Join(appPath, "sample.db")));

            collection.AddCannoliServices<SampleDbContext>();

            var serviceProvider = collection.BuildServiceProvider();

            var cannoliClient = serviceProvider.GetRequiredService<ICannoliClient>();

            cannoliClient.Setup();

            var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();



            await Task.Delay(-1);
        }
    }
}