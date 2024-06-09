using CannoliKit.Modules;
using Discord.WebSocket;

namespace CannoliKit.Extensions
{
    public static class SocketCommandBaseExtensions
    {
        public static async Task RespondAsync(this SocketCommandBase component, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await component.RespondAsync(
                text: moduleComponents.Content,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        public static async Task FollowupAsync(this SocketCommandBase component, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await component.FollowupAsync(
                text: moduleComponents.Content,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        public static async Task ModifyOriginalResponseAsync(this SocketCommandBase component, CannoliModuleBase module)
        {
            var moduleComponents = await module.BuildComponents();
            await component.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
