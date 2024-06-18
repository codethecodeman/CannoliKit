using CannoliKit.Commands;
using CannoliKit.Enums;
using Discord;

namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a Cannoli Command. Handles corresponding Discord command interactions. Automatically registered as a transient service at startup.
    /// </summary>
    public interface ICannoliCommand
    {
        /// <summary>
        /// Discord command name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Deferral type to be used with an incoming Discord interaction.
        /// </summary>
        DeferralType DeferralType { get; }

        /// <summary>
        /// Build Discord command properties. These properties will be used to register the command with Discord.
        /// </summary>
        /// <returns><see cref="ApplicationCommandProperties"/> for this command.</returns>
        Task<ApplicationCommandProperties> BuildAsync();

        /// <summary>
        /// Respond to incoming Discord command interaction.
        /// </summary>
        /// <param name="context">Context for an incoming Discord command interaction.</param>
        Task RespondAsync(CannoliCommandContext context);
    }
}
