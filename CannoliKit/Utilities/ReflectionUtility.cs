using System.Collections.Concurrent;
using System.Reflection;

namespace CannoliKit.Utilities
{
    internal static class ReflectionUtility
    {
        private const BindingFlags AllBindings =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static readonly ConcurrentDictionary<string, Assembly> AssemblyCache = new();
        private static readonly ConcurrentDictionary<string, Type> TypeCache = new();
        private static readonly ConcurrentDictionary<(Type, string), MethodInfo> MethodInfoCache = new();

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

            return MethodInfoCache[(type, methodName)];
        }

        internal static Type? GetType(string assemblyQualifiedTypeName)
        {
            var typeParts = assemblyQualifiedTypeName.Split(',');
            if (typeParts.Length < 2)
            {
                throw new ArgumentException(
                    "Invalid assembly qualified name.",
                    nameof(assemblyQualifiedTypeName));
            }

            var typeName = typeParts[0].Trim();
            var assemblyName = typeParts[1].Trim();
            var key = $"{typeName},{assemblyName}";

            if (TypeCache.TryGetValue(key, out var type))
            {
                return type;
            }

            // Get the assembly by name (ignoring version, culture, and public key token).
            var assembly = GetAssembly(assemblyName);

            // Get the type from the loaded assembly.
            type = assembly.GetType(typeName)!;
            _ = TypeCache.TryAdd(key, type);

            return TypeCache[key];
        }

        private static Assembly GetAssembly(string assemblyName)
        {
            if (AssemblyCache.TryGetValue(assemblyName, out var assembly))
            {
                return assembly;
            }

            var loadedAssembly =
                AppDomain.CurrentDomain.GetAssemblies()
                    .First(x => assemblyName.Equals(x.GetName().Name, StringComparison.OrdinalIgnoreCase));

            _ = AssemblyCache.TryAdd(assemblyName, loadedAssembly);

            return AssemblyCache[assemblyName];
        }
    }


}