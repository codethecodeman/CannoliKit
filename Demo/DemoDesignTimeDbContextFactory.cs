using Demo.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Demo
{
    internal class DemoDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DemoDbContext>
    {
        public DemoDbContext CreateDbContext(string[] args)
        {
            return GenerateDbContext();
        }

        private static DemoDbContext GenerateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DemoDbContext>();

            optionsBuilder.UseSqlite(ConfigurationHelper.GetDbConnectionString());

            return new DemoDbContext(optionsBuilder.Options);
        }
    }
}
