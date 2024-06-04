using CannoliKit.Commands;
using CannoliKit.Enums;

namespace CannoliKit.Interfaces
{
    public interface ICannoliCommand
    {
        DeferralType DeferralType { get; }

        Task RespondAsync(CannoliCommandContext context);
    }
}
