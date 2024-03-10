using CannoliKit.Interfaces;
using CannoliKit.Workers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace CannoliKit.Registries
{
    public sealed class CannoliWorkerRegistry<TContext>
    where TContext : DbContext, ICannoliDbContext
    {
        private readonly CannoliClient<TContext> _cannoliClient;
        private readonly ConcurrentDictionary<Type, CannoliWorkerBase<TContext>> _workers;

        internal CannoliWorkerRegistry(CannoliClient<TContext> cannoliClient)
        {
            _workers = new ConcurrentDictionary<Type, CannoliWorkerBase<TContext>>();
            _cannoliClient = cannoliClient;
        }

        public void Add(CannoliWorkerBase<TContext> worker)
        {
            if (_workers.ContainsKey(worker.GetType())) return;

            worker.Setup(_cannoliClient);
            worker.Log += _cannoliClient.EmitLog;
            _workers[worker.GetType()] = worker;
        }

        public T? GetWorker<T>() where T : CannoliWorkerBase<TContext>
        {
            return _workers.TryGetValue(typeof(T), out var worker)
                ? (T?)worker
                : null;
        }
    }
}
