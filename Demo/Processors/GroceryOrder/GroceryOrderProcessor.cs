using CannoliKit.Attributes;
using CannoliKit.Interfaces;
using Microsoft.Extensions.Logging;

namespace Demo.Processors.GroceryOrder
{
    [CannoliProcessor(maxConcurrentJobs: 1)]
    internal class GroceryOrderProcessor : ICannoliProcessor<GroceryOrderJob>
    {
        private readonly ILogger<GroceryOrderProcessor> _logger;
        public GroceryOrderProcessor(
            DemoDbContext dbContext,
            ICannoliJobQueue<GroceryOrderJob> jobQueue,
            ILogger<GroceryOrderProcessor> logger)
        {
            _logger = logger;
        }

        public async Task HandleJobAsync(GroceryOrderJob job)
        {
            _logger.LogInformation(
                "Received grocery order {id}", job.OrderId);

            await Task.Delay(TimeSpan.FromSeconds(30));

            _logger.LogInformation(
                "Finished grocery order {id}", job.OrderId);
        }
    }
}
