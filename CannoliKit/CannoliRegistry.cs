﻿using System.Collections.Concurrent;

namespace CannoliKit
{
    internal static class CannoliRegistry
    {
        internal static ConcurrentDictionary<string, Type> Commands = new();
    }
}