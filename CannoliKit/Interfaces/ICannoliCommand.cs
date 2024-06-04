using CannoliKit.Commands;

namespace CannoliKit.Interfaces
{
    public interface ICannoliCommand
    {
        Task RespondAsync(CannoliCommandContext context);
    }
}
