namespace CannoliKit.Processors.Jobs
{
    internal sealed class JobWrapper<T>
    {
        internal T Job { get; init; }
        internal TaskCompletionSource TaskCompletionSource { get; init; }

        public JobWrapper(T job, TaskCompletionSource taskCompletionSource)
        {
            Job = job;
            TaskCompletionSource = taskCompletionSource;
        }
    }
}
