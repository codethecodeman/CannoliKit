using CannoliKit.Attributes;
using CannoliKit.Commands;
using CannoliKit.Enums;
using CannoliKit.Interfaces;

namespace Sample.Commands
{
    [CannoliCommand("order-meal", DeferralType.Ephemeral)]

    internal class OrderMealCommand : ICannoliCommand
    {
        public Task RespondAsync(CannoliCommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}
