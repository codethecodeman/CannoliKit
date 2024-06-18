using Demo.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions
{
    internal static class ServiceProviderExtensions
    {
        internal static async Task InitDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
            await context.Database.MigrateAsync();
            await context.SaveChangesAsync();

            await DataHelper.InsertSampleData(context);
            await context.SaveChangesAsync();
        }
    }
}
