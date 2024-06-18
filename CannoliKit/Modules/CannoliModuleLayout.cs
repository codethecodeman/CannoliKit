using Discord;

namespace CannoliKit.Modules
{
    /// <summary>
    /// Layout used to render a Cannoli Module in Discord.
    /// </summary>
    /// <param name="content">Text message.</param>
    /// <param name="embedBuilder">Discord embed.</param>
    /// <param name="componentBuilder">Discord message components.</param>
    public sealed class CannoliModuleLayout(
        string? content = null,
        EmbedBuilder? embedBuilder = null,
        ComponentBuilder? componentBuilder = null)
    {
        /// <summary>
        /// Text message.
        /// </summary>
        public string? Content { get; init; } = content;

        /// <summary>
        /// Discord embed.
        /// </summary>
        public EmbedBuilder? EmbedBuilder { get; init; } = embedBuilder;

        /// <summary>
        /// Discord message components.
        /// </summary>
        public ComponentBuilder? ComponentBuilder { get; init; } = componentBuilder;
    }
}