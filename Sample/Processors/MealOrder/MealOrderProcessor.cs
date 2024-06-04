using CannoliKit.Interfaces;

namespace Sample.Processors.MealOrder
{
    internal class MealOrderProcessor : ICannoliProcessor<MealOrder>
    {
        public MealOrderProcessor(
            SampleDbContext dbContext,
            ICannoliJobQueue<MealOrder> jobQueue)
        {

        }

        public Task HandleJobAsync(MealOrder job)
        {
            throw new NotImplementedException();
        }
    }
}
