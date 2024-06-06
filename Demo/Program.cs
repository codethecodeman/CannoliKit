using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            collection.AddLogging(c =>
            {
                c.AddConsole();
                c.SetMinimumLevel(LogLevel.Information);
            });

            collection.AddSingleton(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
            });

            collection.AddSingleton(new DiscordSocketClient());

            var dbContextFactory = new DemoDesignTimeDbContextFactory();

            collection.AddDbContext<DemoDbContext>(opt =>
                opt.UseSqlite(dbContextFactory.GetConnectionString()));

            collection.AddCannoliServices<DemoDbContext>();

            var serviceProvider = collection.BuildServiceProvider();

            var cannoliClient = serviceProvider.GetRequiredService<ICannoliClient>();

            await cannoliClient.SetupAsync();

            var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

            var json = await File.ReadAllTextAsync(Path.Combine(appPath, "token.json"));
            var jsonDoc = JsonDocument.Parse(json);
            var token = jsonDoc.RootElement.GetProperty("discord-token").GetString();

            await discordClient.LoginAsync(TokenType.Bot, token);
            await discordClient.StartAsync();

            await Task.Delay(-1);
        }
    }
}