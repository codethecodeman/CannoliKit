using CannoliKit.Enums;

namespace CannoliKit.Interfaces
{
    internal interface ICannoliCommand
    {
        DeferralType DeferralType { get; }
    }
}
