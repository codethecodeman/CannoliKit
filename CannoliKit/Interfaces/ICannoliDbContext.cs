using CannoliKit.Models;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Interfaces
{
    public interface ICannoliDbContext
    {
        /// <summary>
        /// Cannoli module save states.
        /// </summary>
        DbSet<CannoliSaveState> CannoliSaveStates { get; set; }

        /// <summary>
        /// Cannoli module routes.
        /// </summary>
        DbSet<CannoliRoute> CannoliRoutes { get; set; }
    }
}
