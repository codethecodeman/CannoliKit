using DisCannoli.Interfaces;
using DisCannoli.Modules;
using DisCannoli.Modules.States;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Extensions
{
    public static class SocketCommandBaseExtensions
    {
        public static async Task RespondAsync<TContext, TState>(this SocketCommandBase component, DisCannoliModule<TContext, TState> module)
            where TContext : DbContext, IDisCannoliDbContext
            where TState : DisCannoliModuleState, new()
        {
            var moduleComponents = await module.BuildComponents();
            await component.RespondAsync(
                text: moduleComponents.Content,
                embeds: moduleComponents.Embeds,
                components: moduleComponents.MessageComponent);
        }

        public static async Task ModifyOriginalResponseAsync<TContext, TState>(this SocketCommandBase component, DisCannoliModule<TContext, TState> module)
            where TContext : DbContext, IDisCannoliDbContext
            where TState : DisCannoliModuleState, new()
        {
            var moduleComponents = await module.BuildComponents();
            await component.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
