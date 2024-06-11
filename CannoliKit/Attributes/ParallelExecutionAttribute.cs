using CannoliKit.Modules;

namespace CannoliKit.Attributes
{
    /// <summary>
    /// Indicates that a method should be executed in parallel.
    /// </summary>
    /// <remarks>
    /// This attribute can be applied to methods of a class which implements <see cref="CannoliModule{TContext,TState}"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ParallelExecutionAttribute : Attribute
    {
    }
}