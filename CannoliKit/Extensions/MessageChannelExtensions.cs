using CannoliKit.Modules;
using Discord.WebSocket;

namespace CannoliKit.Extensions
{
    /// <summary>
    /// CannoliKit extension methods for <see cref="ISocketMessageChannel"/>.
    /// </summary>
    public static class MessageChannelExtensions
    {
        /// <summary>
        /// Send a message to a channel with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="channel">Discord message channel.</param>
        /// <param name="module">Module to be used in the response.</param>
        public static async Task SendMessageAsync(this ISocketMessageChannel channel, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();

            await channel.SendMessageAsync(
                text: moduleComponents.Content,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }
    }
}
