using CannoliKit.Interfaces;
using CannoliKit.Models;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Modules
{
    internal class CannoliModuleRouter<TContext> : ICannoliModuleRouter
        where TContext : DbContext, ICannoliDbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TContext _dbContext;

        public CannoliModuleRouter(
            IServiceProvider serviceProvider,
            TContext dbContext)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
        }

        public async Task RouteToModuleCallback(CannoliRoute route, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
