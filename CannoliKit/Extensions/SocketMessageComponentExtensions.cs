using CannoliKit.Modules;
using Discord.WebSocket;

namespace CannoliKit.Extensions
{
    /// <summary>
    /// CannoliKit extension methods for <see cref="SocketMessageComponent"/>.
    /// </summary>
    public static class SocketMessageComponentExtensions
    {
        /// <summary>
        /// Modify the interaction's original response with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="component">Discord interaction.</param>
        /// <param name="module">Module to be used in the modification.</param>
        public static async Task ModifyOriginalResponseAsync(this SocketMessageComponent component, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await component.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
