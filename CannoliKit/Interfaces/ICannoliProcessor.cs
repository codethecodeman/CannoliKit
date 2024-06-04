namespace CannoliKit.Interfaces
{
    public interface ICannoliProcessor<in T>
    {
        Task HandleJobAsync(T job);
    }
}
