using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sample
{
    internal class DemoDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DemoDbContext>
    {
        public DemoDbContext CreateDbContext(string[] args)
        {
            return GenerateDbContext();
        }

        public DemoDbContext CreateDbContext()
        {
            return GenerateDbContext();
        }

        internal string GetConnectionString()
        {
            var path = AppContext.BaseDirectory;
            var dbPath = Path.Join(path, "demo.db");
            return $"Data Source={dbPath}";
        }

        private DemoDbContext GenerateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DemoDbContext>();

            optionsBuilder.UseSqlite();

            return new DemoDbContext(optionsBuilder.Options);
        }
    }
}
