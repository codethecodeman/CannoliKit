using CannoliKit.Models;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Interfaces
{
    public interface ICannoliDbContext
    {
        /// <summary>
        /// Cannoli module save states.
        /// </summary>
        DbSet<SaveState> CannoliSaveStates { get; set; }

        /// <summary>
        /// Cannoli module routes.
        /// </summary>
        DbSet<Route> CannoliRoutes { get; set; }
    }
}
