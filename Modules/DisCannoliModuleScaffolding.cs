using Discord;

namespace DisCannoli.Modules
{
    public class DisCannoliModuleScaffolding
    {
        public string? Content { get; set; }
        public EmbedBuilder? EmbedBuilder { get; }
        public ComponentBuilder? ComponentBuilder { get; }

        public DisCannoliModuleScaffolding(
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
