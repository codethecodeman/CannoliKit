using Discord.WebSocket;

namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a Cannoli Command that uses autocomplete. Handles corresponding Discord autocomplete interactions.
    /// </summary>
    public interface ICannoliAutocompleteCommand
    {
        /// <summary>
        /// Respond to incoming Discord autocomplete interaction.
        /// </summary>
        /// <param name="interaction">Autocomplete interaction.</param>
        public Task AutocompleteAsync(SocketAutocompleteInteraction interaction);
    }
}
