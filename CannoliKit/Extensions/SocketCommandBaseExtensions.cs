using CannoliKit.Interfaces;
using CannoliKit.Modules;
using CannoliKit.Modules.States;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Extensions
{
    public static class SocketCommandBaseExtensions
    {
        public static async Task RespondAsync<TContext, TState>(this SocketCommandBase component, CannoliModule<TContext, TState> module)
            where TContext : DbContext, ICannoliDbContext
            where TState : CannoliModuleState, new()
        {
            var moduleComponents = await module.BuildComponents();
            await component.RespondAsync(
                text: moduleComponents.Content,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        public static async Task ModifyOriginalResponseAsync<TContext, TState>(this SocketCommandBase component, CannoliModule<TContext, TState> module)
            where TContext : DbContext, ICannoliDbContext
            where TState : CannoliModuleState, new()
        {
            var moduleComponents = await module.BuildComponents();
            await component.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
