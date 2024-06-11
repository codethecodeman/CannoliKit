using CannoliKit.Models;
using Microsoft.EntityFrameworkCore;

namespace CannoliKit.Interfaces
{
    /// <summary>
    /// Represents a <see cref="DbContext"/> that contains sets necessary for CannoliKit to function.
    /// </summary>
    public interface ICannoliDbContext
    {
        /// <summary>
        /// Cannoli Module save states.
        /// </summary>
        DbSet<CannoliSaveState> CannoliSaveStates { get; set; }

        /// <summary>
        /// Cannoli Module routes.
        /// </summary>
        DbSet<CannoliRoute> CannoliRoutes { get; set; }
    }
}
