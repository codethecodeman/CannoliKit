using DisCannoli.Models;
using Microsoft.EntityFrameworkCore;

namespace DisCannoli.Interfaces
{
    public interface IDisCannoliDbContext
    {
        DbSet<SaveState> CannoliSaveStates { get; set; }
        DbSet<Route> CannoliRoutes { get; set; }
    }
}
