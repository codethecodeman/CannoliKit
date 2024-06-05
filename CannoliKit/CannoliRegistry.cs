using CannoliKit.Attributes;
using System.Collections.Concurrent;

namespace CannoliKit
{
    internal class CannoliRegistry
    {
        internal ConcurrentDictionary<string, Type> Commands { get; } = [];
        internal ConcurrentDictionary<string, CannoliCommandAttribute> CommandAttributes { get; } = [];
    }
}
