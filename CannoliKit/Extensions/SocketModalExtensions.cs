using CannoliKit.Interfaces;
using CannoliKit.Modules;
using CannoliKit.Modules.States;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Extensions
{
    public static class SocketModalExtensions
    {
        public static async Task ModifyOriginalResponseAsync<TContext, TState>(this SocketModal modal, CannoliModule<TContext, TState> module)
            where TContext : DbContext, ICannoliDbContext
            where TState : CannoliModuleState, new()
        {
            var moduleComponents = await module.BuildComponents();
            await modal.ModifyOriginalResponseAsync(moduleComponents.ApplyToMessageProperties);
        }
    }
}
