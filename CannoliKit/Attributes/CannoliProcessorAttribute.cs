using CannoliKit.Interfaces;

namespace CannoliKit.Attributes
{
    /// <summary>
    /// Configure behavior for a class that implements <see cref="ICannoliProcessor{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CannoliProcessorAttribute : Attribute
    {
        /// <summary>
        /// Maximum number of concurrent jobs that this processor may handle at once.
        /// </summary>
        public int MaxConcurrentJobs { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CannoliProcessorAttribute"/>.
        /// </summary>
        /// <param name="maxConcurrentJobs">Maximum number of concurrent jobs that this processor may handle at once.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CannoliProcessorAttribute(int maxConcurrentJobs)
        {
            if (maxConcurrentJobs <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxConcurrentJobs),
                    maxConcurrentJobs,
                    "Value must be greater than zero.");
            }

            MaxConcurrentJobs = maxConcurrentJobs;
        }
    }
}
