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
        private ILogger<DiscordSocketClient> _discordSocketClientLogger = null!;

        public static Task Main() => new Program().MainAsync();

        internal async Task MainAsync()
        {
            var collection = new ServiceCollection();

            collection.AddLogging(c =>
            {
                c.SetMinimumLevel(LogLevel.Information);
                c.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                });
            });

            collection.AddSingleton(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
            });

            collection.AddSingleton(new DiscordSocketClient());

            collection.AddDbContext<DemoDbContext>(opt =>
                opt.UseSqlite(ConfigurationHelper.GetDbConnectionString())
                    .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug))));

            collection.AddCannoliServices<DemoDbContext>();

            var serviceProvider = collection.BuildServiceProvider();

            _discordSocketClientLogger = serviceProvider.GetRequiredService<ILogger<DiscordSocketClient>>();

            await serviceProvider.InitDatabaseAsync();

            var cannoliClient = serviceProvider.GetRequiredService<ICannoliClient>();

            await cannoliClient.SetupAsync();

            var discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();

            discordClient.Log += m => _discordSocketClientLogger.HandleLogMessage(m);

            await discordClient.LoginAsync(TokenType.Bot, ConfigurationHelper.GetDiscordToken());
            await discordClient.StartAsync();

            await Task.Delay(-1);
        }
    }
}