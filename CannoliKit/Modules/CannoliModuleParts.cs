using Discord;

namespace CannoliKit.Modules
{
    public sealed class CannoliModuleParts(
        string? content = null,
        EmbedBuilder? embedBuilder = null,
        ComponentBuilder? componentBuilder = null)
    {
        public string? Content { get; init; } = content;
        public EmbedBuilder? EmbedBuilder { get; init; } = embedBuilder;
        public ComponentBuilder? ComponentBuilder { get; init; } = componentBuilder;
    }
}