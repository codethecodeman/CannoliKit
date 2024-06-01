using CannoliKit.Enums;

namespace CannoliKit.Interfaces
{
    internal interface ICannoliWorkerChannel<T>
    {
        void Write(T item, Priority priority);
        Task<T> ReadAsync();
        void Dispose();
    }
}
