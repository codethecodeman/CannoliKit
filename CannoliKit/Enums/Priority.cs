using CannoliKit.Interfaces;

namespace CannoliKit.Enums
{
    /// <summary>
    /// Specifies a priority type to be used with jobs submitted to an <see cref="ICannoliJobQueue{TJob}"/>.
    /// </summary>
    public enum Priority
    {
        /// <summary>
        /// Normal priority.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// High priority. Takes precedence over normal priority.
        /// </summary>
        High = 2,
    }
}
