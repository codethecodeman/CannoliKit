using CannoliKit.Interfaces;
using CannoliKit.Models;
using Microsoft.EntityFrameworkCore;

namespace Sample
{
    internal class SampleDbContext : DbContext, ICannoliDbContext
    {
        public SampleDbContext(DbContextOptions options) : base(options) { }
        public DbSet<CannoliSaveState> CannoliSaveStates { get; set; } = null!;
        public DbSet<CannoliRoute> CannoliRoutes { get; set; } = null!;
    }
}
