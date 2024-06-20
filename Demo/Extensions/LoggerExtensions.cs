using Discord;
using Microsoft.Extensions.Logging;

namespace Demo.Extensions
{
    internal static class LoggerExtensions
    {
        internal static async Task HandleLogMessage(this ILogger logger, LogMessage msg)
        {
            var severity = msg.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Trace,
                LogSeverity.Debug => LogLevel.Debug,
                _ => LogLevel.Information
            };

            logger.Log(severity, msg.Exception, msg.Message);

            await Task.CompletedTask;
        }
    }
}
