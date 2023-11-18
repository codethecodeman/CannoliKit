using DisCannoli.Interfaces;
using DisCannoli.Modules;
using DisCannoli.Modules.States;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Extensions
{
    public static class SocketMessageComponentExtensions
    {
        public static async Task ModifyOriginalResponseAsync<TContext, TState>(this SocketMessageComponent component, DisCannoliModule<TContext, TState> module)
            where TContext : DbContext, IDisCannoliDbContext
            where TState : DisCannoliModuleState, new()
        {
            var moduleComponents = await module.BuildComponents();
            await component.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
