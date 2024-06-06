using CannoliKit.Commands;
using CannoliKit.Enums;
using Discord;

namespace CannoliKit.Interfaces
{
    public interface ICannoliCommand
    {
        string Name { get; }
        DeferralType DeferralType { get; }
        Task RespondAsync(CannoliCommandContext context);
        Task<ApplicationCommandProperties> BuildAsync();
    }
}
