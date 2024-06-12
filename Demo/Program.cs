using CannoliKit.Extensions;
using CannoliKit.Interfaces;
using Demo.Extensions;
using Demo.Helpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo
{
    public class Program
    {
        public static Task Main() => new Program().MainAsync();

        internal async Task MainAsync()
        {
            var collection = new ServiceCollection();

            collection.AddDbContext<DemoDbContext>(opt =>
                opt.UseSqlite(ConfigurationHelper.GetDbConnectionString())
                    .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug))));

            collection.AddSingleton(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
            });

            collection.AddSingleton<DiscordSocketClient>();

            collection.AddCannoliServices<DemoDbContext>();

            collection.AddLogging(c =>
            {
                c.SetMinimumLevel(LogLevel.Information);
                c.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                });
            });

            var serviceProvider = collection.BuildServiceProvider();

            await serviceProvider.InitDatabaseAsync();

            var cannoliClient = serviceProvider.GetRequiredService<ICannoliClient>();

            await cannoliClient.SetupAsync();

            var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

            var discordClientLogger = serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();

            discordClient.Log += m => discordClientLogger.HandleLogMessage(m);

            await discordClient.LoginAsync(TokenType.Bot, ConfigurationHelper.GetDiscordToken());
            await discordClient.StartAsync();

            await Task.Delay(-1);
        }
    }
}