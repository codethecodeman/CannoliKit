using CannoliKit.Interfaces;
using CannoliKit.Models;
using Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo
{
    internal class DemoDbContext : DbContext, ICannoliDbContext
    {
        public DemoDbContext(DbContextOptions options) : base(options) { }
        public DbSet<CannoliSaveState> CannoliSaveStates { get; set; } = null!;
        public DbSet<CannoliRoute> CannoliRoutes { get; set; } = null!;
        public DbSet<FoodItem> FoodItems { get; set; } = null!;
        public DbSet<MealOrder> MealOrders { get; set; } = null!;
        public DbSet<MealOrderItem> MealOrderItems { get; set; } = null!;
    }
}
