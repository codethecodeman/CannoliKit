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

            return methodInfo;
        }

        internal static Type? GetType(string assemblyQualifiedTypeName)
        {
            var typeParts = assemblyQualifiedTypeName.Split(',');
            if (typeParts.Length < 2)
            {
                throw new ArgumentException("Invalid assembly qualified name.", nameof(assemblyQualifiedTypeName));
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

            return type;
        }

        private static Assembly GetAssembly(string assemblyName)
        {
            if (AssemblyCache.TryGetValue(assemblyName, out var assembly))
            {
                return assembly;
            }

            var loadedAssemblies =
                AppDomain.CurrentDomain.GetAssemblies();

            // Load the assembly by its name without version information.
            foreach (var loadedAssembly in loadedAssemblies)
            {
                var loadedAssemblyName = loadedAssembly.GetName().Name;
                if (assemblyName.Equals(loadedAssemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    _ = AssemblyCache.TryAdd(assemblyName, loadedAssembly);
                    return loadedAssembly;
                }
            }

            // If the assembly is not referenced, try to load it directly (this may fail).
            try
            {
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch
            {
                return null;
            }
        }
    }


}