using CannoliKit.Enums;

namespace CannoliKit.Interfaces
{
    internal interface ICannoliJobQueueChannel<T>
    {
        void Write(T item, Priority priority);
        Task<T> ReadAsync();
        void Dispose();
    }
}
