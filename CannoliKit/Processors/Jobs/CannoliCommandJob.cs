using Discord.WebSocket;

namespace CannoliKit.Processors.Jobs
{
    internal sealed class CannoliCommandJob
    {
        internal string CommandName { get; init; } = null!;
        internal SocketCommandBase? Command { get; init; } = null!;
        internal SocketAutocompleteInteraction? Autocomplete { get; init; } = null!;
    }
}
