using System.Collections.Concurrent;
using System.Reflection;

namespace DisCannoli.Utilities
{
    internal static class ReflectionUtility
    {
        private const BindingFlags AllBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static readonly ConcurrentDictionary<(Type, string), MethodInfo> MethodInfoCache = new();
        private static readonly ConcurrentDictionary<string, Type> TypeCache = new();

        internal static MethodInfo? GetMethodInfo(Type type, string methodName)
        {
            if (MethodInfoCache.TryGetValue((type, methodName), out var methodInfo))
            {
                return methodInfo;
            }

            methodInfo = type.GetMethod(methodName, AllBindings);

            if (methodInfo == null)
            {
                return null;
            }

            _ = MethodInfoCache.TryAdd((type, methodName), methodInfo);

            return methodInfo;
        }

        internal static Type? GetType(string typeName)
        {
            if (TypeCache.TryGetValue(typeName, out var type))
            {
                return type;
            }

            type = Type.GetType(typeName);

            if (type == null)
            {
                return null;
            }

            _ = TypeCache.TryAdd(typeName, type);

            return type;
        }
    }
}