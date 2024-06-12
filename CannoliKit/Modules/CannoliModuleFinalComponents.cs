using Discord;

namespace CannoliKit.Modules
{

    internal sealed class CannoliModuleFinalComponents
    {
        public string? Content { get; set; }
        public Embed[]? Embeds { get; set; }
        public MessageComponent? MessageComponent { get; set; }

        internal CannoliModuleFinalComponents(string? content, Embed[]? embeds, MessageComponent? messageComponent = null)
        {
            Content = content;
            Embeds = embeds;
            MessageComponent = messageComponent;
        }

        internal void ApplyToMessageProperties(MessageProperties messageProperties)
        {
            messageProperties.Embeds = Embeds;
            messageProperties.Components = MessageComponent;
            messageProperties.Content = Content;
        }
    }
}
