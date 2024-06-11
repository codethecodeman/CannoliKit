using CannoliKit.Modules;
using Discord.WebSocket;

namespace CannoliKit.Extensions
{
    /// <summary>
    /// CannoliKit extension methods for <see cref="SocketCommandBase"/>.
    /// </summary>
    public static class SocketCommandBaseExtensions
    {
        /// <summary>
        /// Respond to the interaction with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="command">Discord interaction.</param>
        /// <param name="module">Module to be used in the response.</param>
        public static async Task RespondAsync(this SocketCommandBase command, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await command.RespondAsync(
                text: moduleComponents.Content,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        /// <summary>
        /// Followup to the interaction with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="command">Discord interaction.</param>
        /// <param name="module">Module to be used in the followup.</param>
        public static async Task FollowupAsync(this SocketCommandBase command, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await command.FollowupAsync(
                text: moduleComponents.Content,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        /// <summary>
        /// Modify the interaction's original response with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="command">Discord interaction.</param>
        /// <param name="module">Module to be used in the modification.</param>
        public static async Task ModifyOriginalResponseAsync(this SocketCommandBase command, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await command.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
