using DisCannoli.Interfaces;
using DisCannoli.Modules;
using DisCannoli.Modules.States;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Extensions
{
    public static class SocketModalExtensions
    {
        public static async Task ModifyOriginalResponseAsync<TContext, TState>(this SocketModal modal, DisCannoliModule<TContext, TState> module)
            where TContext : DbContext, IDisCannoliDbContext
            where TState : DisCannoliModuleState, new()
        {
            var moduleComponents = await module.BuildComponents();
            await modal.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
