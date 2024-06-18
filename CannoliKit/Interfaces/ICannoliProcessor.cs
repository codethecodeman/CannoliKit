namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a Cannoli Processor. Handles jobs of the specified type. Automatically registered as a transient service at startup.
    /// A new Cannoli Processor is initialized per job, and dependencies within are resolved using a unique Dependency Injection scope.
    /// </summary>
    /// <typeparam name="T">Job type to be handled.</typeparam>
    /// <seealso cref="ICannoliJobQueue{TJob}"/>
    public interface ICannoliProcessor<in T>
    {
        /// <summary>
        /// Handle an incoming job from a job queue.
        /// </summary>
        /// <param name="job">Job to be handled.</param>
        Task HandleJobAsync(T job);
    }
}
