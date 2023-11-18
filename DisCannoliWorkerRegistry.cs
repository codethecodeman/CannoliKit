using DisCannoli.Workers;
using System.Collections.Concurrent;

namespace DisCannoli
{
    public class DisCannoliWorkerRegistry
    {
        private readonly DisCannoliClient _cannoliClient;
        private readonly ConcurrentDictionary<Type, DisCannoliWorkerBase> _workers;

        internal DisCannoliWorkerRegistry(DisCannoliClient cannoliClient)
        {
            _workers = new ConcurrentDictionary<Type, DisCannoliWorkerBase>();
            _cannoliClient = cannoliClient;
        }

        public void Add(DisCannoliWorkerBase worker)
        {
            if (_workers.ContainsKey(worker.GetType())) return;

            worker.Setup(_cannoliClient);
            worker.Log += _cannoliClient.EmitLog;
            _workers[worker.GetType()] = worker;
        }

        public T? GetWorker<T>() where T : DisCannoliWorkerBase
        {
            return _workers.TryGetValue(typeof(T), out var worker)
                ? (T?)worker
                : null;
        }
    }
}
