using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

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

            discordClient.LoggedIn += async () =>
            {
                cannoliClient.Setup();
            };

            var json = await File.ReadAllTextAsync(Path.Combine(appPath, "token.json"));
            var jsonDoc = JsonDocument.Parse(json);
            var token = jsonDoc.RootElement.GetProperty("discord-token").GetString();

            await discordClient.LoginAsync(TokenType.Bot, token);

            await Task.Delay(-1);
        }
    }
}