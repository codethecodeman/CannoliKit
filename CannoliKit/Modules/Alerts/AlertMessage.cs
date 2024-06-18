using Discord;

namespace CannoliKit.Modules.Alerts
{
    internal sealed class AlertMessage
    {
        internal Emoji Emoji { get; init; } = null!;
        internal string Content { get; init; } = null!;
        internal Color Color { get; init; }

        public override string ToString()
        {
            return $"{Emoji} {Content}";
        }
    }
}
