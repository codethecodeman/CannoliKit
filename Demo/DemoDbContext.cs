using CannoliKit.Interfaces;
using CannoliKit.Models;
using Microsoft.EntityFrameworkCore;
using Sample.Models;

namespace Sample
{
    internal class DemoDbContext : DbContext, ICannoliDbContext
    {
        public DemoDbContext()
        {
            base.
        }
        public DemoDbContext(DbContextOptions options) : base(options) { }
        public DbSet<CannoliSaveState> CannoliSaveStates { get; set; } = null!;
        public DbSet<CannoliRoute> CannoliRoutes { get; set; } = null!;
        public DbSet<FoodItem> FoodItems { get; set; } = null!;
        public DbSet<FoodItem> MealOrders { get; set; } = null!;
    }
}
