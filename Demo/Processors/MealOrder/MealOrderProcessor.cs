using CannoliKit.Attributes;
using CannoliKit.Interfaces;

namespace Sample.Processors.MealOrder
{
    [CannoliProcessor(maxConcurrentJobs: 4)]
    internal class MealOrderProcessor : ICannoliProcessor<MealOrderJob>
    {
        public MealOrderProcessor(
            DemoDbContext dbContext,
            ICannoliJobQueue<MealOrderJob> jobQueue)
        {

        }

        public Task HandleJobAsync(MealOrderJob job)
        {
            throw new NotImplementedException();
        }
    }
}
