using CannoliKit.Commands;
using System.Collections.Concurrent;

namespace CannoliKit
{
    internal class CannoliRegistry
    {
        internal ConcurrentDictionary<string, CannoliCommandMeta> Commands { get; } = [];
    }
}
