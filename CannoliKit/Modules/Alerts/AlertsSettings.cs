using Discord;

namespace CannoliKit.Modules.Alerts
{
    /// <summary>
    /// Handles alert message settings for a Cannoli Module.
    /// </summary>
    public sealed class AlertsSettings
    {
        internal AlertMessage? InfoMessage { get; private set; }
        internal AlertMessage? ErrorMessage { get; private set; }

        /// <summary>
        /// Set an info message to appear on next module refresh.
        /// </summary>
        /// <param name="content">Message content.</param>
        /// <param name="emoji">Emoji. Default is "Information".</param>
        /// <param name="color">Embed color. Default is "LightGrey".</param>
        public void SetInfoMessage(string content, Emoji? emoji = null, Color? color = null)
        {
            InfoMessage = new AlertMessage
            {
                Content = content,
                Emoji = emoji ?? new Emoji("ℹ️"),
                Color = color ?? Color.LightGrey
            };
        }

        /// <summary>
        /// Set an error message to appear on next module refresh.
        /// </summary>
        /// <param name="content">Message content.</param>
        /// <param name="emoji">Emoji. Default is "Cross Mark".</param>
        /// <param name="color">Embed color. Default is "Red".</param>
        public void SetErrorMessage(string content, Emoji? emoji = null, Color? color = null)
        {
            ErrorMessage = new AlertMessage
            {
                Content = content,
                Emoji = emoji ?? new Emoji("❌"),
                Color = color ?? Color.Red
            };
        }
    }

}
