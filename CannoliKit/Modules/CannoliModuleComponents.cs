using Discord;

namespace CannoliKit.Modules
{
    public sealed class CannoliModuleComponents
    {
        public string? Content { get; set; }
        public Embed[]? Embeds { get; set; }
        public MessageComponent? MessageComponent { get; set; }

        internal CannoliModuleComponents(string? content, Embed[]? embeds, MessageComponent? messageComponent = null)
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
