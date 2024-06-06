using System.Text.Json;

namespace Demo.Helpers
{
    internal static class ConfigurationHelper
    {
        internal static string GetDbConnectionString()
        {
            var path = Path.Combine(
                AppContext.BaseDirectory,
                "demo.db");

            return $"Data Source={path}";
        }

        internal static string GetDiscordToken()
        {
            var json = File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "token.json"));

            var jsonDoc = JsonDocument.Parse(json);

            return jsonDoc.RootElement.GetProperty("discord-token").GetString()!;
        }
    }
}
