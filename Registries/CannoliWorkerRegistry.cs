using CannoliKit.Workers;
using System.Collections.Concurrent;

namespace CannoliKit.Registries
{
    public sealed class CannoliWorkerRegistry
    {
        private readonly CannoliClient _cannoliClient;
        private readonly ConcurrentDictionary<Type, CannoliWorkerBase> _workers;

        internal CannoliWorkerRegistry(CannoliClient cannoliClient)
        {
            _workers = new ConcurrentDictionary<Type, CannoliWorkerBase>();
            _cannoliClient = cannoliClient;
        }

        public void Add(CannoliWorkerBase worker)
        {
            if (_workers.ContainsKey(worker.GetType())) return;

            worker.Setup(_cannoliClient);
            worker.Log += _cannoliClient.EmitLog;
            _workers[worker.GetType()] = worker;
        }

        public T? GetWorker<T>() where T : CannoliWorkerBase
        {
            return _workers.TryGetValue(typeof(T), out var worker)
                ? (T?)worker
                : null;
        }
    }
}
