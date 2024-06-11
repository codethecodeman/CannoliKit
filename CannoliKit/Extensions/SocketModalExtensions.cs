using CannoliKit.Modules;
using Discord.WebSocket;

namespace CannoliKit.Extensions
{
    /// <summary>
    /// CannoliKit extension methods for <see cref="SocketModal"/>.
    /// </summary>
    public static class SocketModalExtensions
    {
        /// <summary>
        /// Modify the modal's original response with a <see cref="CannoliModule{TContext,TState}"/>.
        /// </summary>
        /// <param name="modal">Discord modal.</param>
        /// <param name="module">Module to be used in the modification.</param>
        public static async Task ModifyOriginalResponseAsync(this SocketModal modal, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await modal.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
