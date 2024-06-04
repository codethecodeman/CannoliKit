using CannoliKit.Enums;

namespace CannoliKit.Interfaces
{
    public interface ICannoliJobQueue<in TJob>
    {
        void EnqueueJob(TJob job, Priority priority = Priority.Normal);
        void ScheduleRepeatingJob(TimeSpan repeatEvery, TJob job, bool doWorkNow);
    }
}
