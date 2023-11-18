using Discord;

namespace DisCannoli.Modules
{
    public class DisCannoliModuleComponents
    {
        public string? Content { get; set; }
        public Embed[]? Embeds { get; set; }
        public MessageComponent? MessageComponent { get; set; }

        public DisCannoliModuleComponents(string? content, Embed[]? embeds, MessageComponent? messageComponent = null)
        {
            Content = content;
            Embeds = embeds;
            MessageComponent = messageComponent;
        }

        public void ApplyToMessageProperties(MessageProperties messageProperties)
        {
            messageProperties.Embeds = Embeds;
            messageProperties.Components = MessageComponent;
            messageProperties.Content = Content;
        }
    }
}
