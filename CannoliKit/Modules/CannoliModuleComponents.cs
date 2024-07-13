using Discord;

namespace CannoliKit.Modules
{

    /// <summary>
    /// Represents Discord message components for a built <see cref="CannoliModule{TContext,TState}"/>.
    /// </summary>
    public sealed class CannoliModuleComponents
    {
        /// <summary>
        /// String content for a Discord message.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Embeds for a Discord message.
        /// </summary>
        public Embed[]? Embeds { get; set; }

        /// <summary>
        /// Message components for a Discord message.
        /// </summary>
        public MessageComponent? MessageComponent { get; set; }

        internal CannoliModuleComponents(string? text, Embed[]? embeds, MessageComponent? messageComponent = null)
        {
            Text = text;
            Embeds = embeds;
            MessageComponent = messageComponent;
        }

        internal void ApplyToMessageProperties(MessageProperties messageProperties)
        {
            messageProperties.Embeds = Embeds;
            messageProperties.Components = MessageComponent;
            messageProperties.Content = Text;
        }
    }
}
