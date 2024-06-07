using System.Collections.Concurrent;

namespace CannoliKit.Concurrency
{
    internal sealed class TurnManager
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _turns = new();

        internal TaskCompletionSource<bool>? GetTurnToAwait(string stateId, TaskCompletionSource<bool> nextTurn)
        {
            lock (_turns)
            {
                TaskCompletionSource<bool>? currentTurn = null;

                if (_turns.TryGetValue(stateId, out var tcs))
                {
                    currentTurn = tcs;
                }

                _turns[stateId] = nextTurn;

                return currentTurn;
            }
        }

        internal void CleanupTurns()
        {
            lock (_turns)
            {
                var completedEntries = _turns
                    .Where(x => x.Value.Task.IsCompleted)
                    .ToList();

                foreach (var completedEntry in completedEntries)
                {
                    _turns.TryRemove(completedEntry.Key, out _);
                }
            }
        }
    }
}
