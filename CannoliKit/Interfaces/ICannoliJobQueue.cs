using CannoliKit.Enums;

namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a Cannoli Processor job queue. Automatically registered as a singleton service at startup.
    /// Per job, the job queue will initialize a corresponding processor with a unique Dependency Injection scope.
    /// </summary>
    /// <typeparam name="TJob"></typeparam>
    /// <seealso cref="ICannoliProcessor{T}"/>
    public interface ICannoliJobQueue<in TJob>
    {
        /// <summary>
        /// Enqueue a new job to be handled by the corresponding <see cref="ICannoliProcessor{T}"/>.
        /// </summary>
        /// <param name="job">Job to enqueue.</param>
        /// <param name="priority">Job priority.</param>
        /// <returns>Task completion source that will complete when the job is finished executing.</returns>
        TaskCompletionSource EnqueueJob(TJob job, Priority priority = Priority.Normal);

        /// <summary>
        /// Schedule a repeating job to be handled by the corresponding <see cref="ICannoliProcessor{T}"/>.
        /// </summary>
        /// <param name="repeatEvery">Time between scheduled job runs.</param>
        /// <param name="job">Job to enqueue.</param>
        /// <param name="doWorkNow">If true, the job will enqueue immediately. If false, the first job will not enqueue until the specified time has passed.</param>
        void ScheduleRepeatingJob(TimeSpan repeatEvery, TJob job, bool doWorkNow);
    }
}
