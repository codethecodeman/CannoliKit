namespace CannoliKit.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CannoliProcessorAttribute : Attribute
    {
        public int MaxConcurrentJobs { get; }

        public CannoliProcessorAttribute(int maxConcurrentJobs)
        {
            MaxConcurrentJobs = maxConcurrentJobs;
        }
    }
}
