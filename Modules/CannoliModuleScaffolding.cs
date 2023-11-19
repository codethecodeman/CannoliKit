using Discord;

namespace CannoliKit.Modules
{
    public class CannoliModuleScaffolding
    {
        public string? Content { get; set; }
        public EmbedBuilder? EmbedBuilder { get; }
        public ComponentBuilder? ComponentBuilder { get; }

        public CannoliModuleScaffolding(
            string? content = null,
            EmbedBuilder? embedBuilder = null,
            ComponentBuilder? componentBuilder = null)
        {
            Content = content;
            EmbedBuilder = embedBuilder;
            ComponentBuilder = componentBuilder;
        }
    }
}
