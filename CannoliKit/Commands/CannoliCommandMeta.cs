using CannoliKit.Enums;
using Discord;

namespace CannoliKit.Commands
{
    internal sealed class CannoliCommandMeta
    {
        internal string Name { get; init; }
        internal DeferralType DeferralType { get; init; }
        internal ApplicationCommandProperties ApplicationCommandProperties { get; init; }
        internal Type Type { get; init; }
    }
}
