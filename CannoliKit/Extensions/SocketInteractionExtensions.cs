using CannoliKit.Modules;
using Discord.WebSocket;

namespace CannoliKit.Extensions
{
    /// <summary>
    /// CannoliKit extension methods for <see cref="SocketInteraction"/>.
    /// </summary>
    public static class SocketInteractionExtensions
    {
        /// <summary>
        /// Respond to the interaction with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="interaction">Discord interaction.</param>
        /// <param name="module">Module to be used in the response.</param>
        public static async Task RespondAsync(this SocketInteraction interaction, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponentsAsync();
            await interaction.RespondAsync(
                text: moduleComponents.Text,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        /// <summary>
        /// Followup to the interaction with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="interaction">Discord interaction.</param>
        /// <param name="module">Module to be used in the followup.</param>
        public static async Task FollowupAsync(this SocketInteraction interaction, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponentsAsync();
            await interaction.FollowupAsync(
                text: moduleComponents.Text,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        /// <summary>
        /// Modify the interaction's original response with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="interaction">Discord interaction.</param>
        /// <param name="module">Module to be used in the modification.</param>
        public static async Task ModifyOriginalResponseAsync(this SocketInteraction interaction, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponentsAsync();
            await interaction.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
