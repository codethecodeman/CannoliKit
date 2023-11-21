using Discord;

namespace CannoliKit.Modules
{
    public class CannoliModuleScaffolding(
        string? content = null,
        EmbedBuilder? embedBuilder = null,
        ComponentBuilder? componentBuilder = null)
    {
        public string? Content { get; } = content;
        public EmbedBuilder? EmbedBuilder { get; } = embedBuilder;
        public ComponentBuilder? ComponentBuilder { get; } = componentBuilder;
    }
}
