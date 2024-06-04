namespace CannoliKit.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CannoliProcessorAttribute : Attribute
    {
        public readonly int MaxConcurrentJobs;

        public CannoliProcessorAttribute(int maxConcurrentJobs)
        {
            MaxConcurrentJobs = maxConcurrentJobs;
        }
    }
}
