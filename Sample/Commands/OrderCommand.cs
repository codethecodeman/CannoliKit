using CannoliKit.Attributes;
using CannoliKit.Commands;
using CannoliKit.Enums;
using CannoliKit.Interfaces;

namespace Sample.Commands
{
    [CannoliCommandName("order")]
    internal class OrderCommand : ICannoliCommand
    {
        public DeferralType DeferralType => DeferralType.Ephemeral;
        public Task RespondAsync(CannoliCommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}
