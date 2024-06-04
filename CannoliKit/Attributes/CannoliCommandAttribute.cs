using CannoliKit.Enums;

namespace CannoliKit.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CannoliCommandAttribute : Attribute
    {
        public readonly string CommandName;
        public readonly DeferralType DeferralType;
        public CannoliCommandAttribute(string commandName, DeferralType deferralType)
        {
            CommandName = commandName;
            DeferralType = deferralType;
        }
    }
}
