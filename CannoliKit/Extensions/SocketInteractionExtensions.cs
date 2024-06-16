using CannoliKit.Modules;
using Discord.WebSocket;

namespace CannoliKit.Extensions
{
    /// <summary>
    /// CannoliKit extensions for SocketInteraction.
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
            var moduleComponents = await module.BuildComponents();
            await interaction.RespondAsync(
                text: moduleComponents.Content,
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
            var moduleComponents = await module.BuildComponents();
            await interaction.FollowupAsync(
                text: moduleComponents.Content,
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
            var moduleComponents = await module.BuildComponents();
            await interaction.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
