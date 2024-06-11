using CannoliKit.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CannoliKit.Models
{
    /// <summary>
    /// Represents a Cannoli Route. Directs interactions for Cannoli Modules.
    /// </summary>
    [Index(nameof(StateId))]
    [Index(nameof(Id), nameof(Type))]
    public sealed class CannoliRoute
    {
        /// <summary>
        /// Cannoli Route ID.
        /// </summary>
        [Key]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Cannoli Route name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Cannoli Route type.
        /// </summary>
        [Required]
        public RouteType Type { get; set; }

        /// <summary>
        /// Callback type for use with Reflection.
        /// </summary>
        [Required]
        public string CallbackType { get; set; } = null!;

        /// <summary>
        /// Callback method for use with Reflection.
        /// </summary>
        [Required]
        public string CallbackMethod { get; set; } = null!;

        /// <summary>
        /// Cannoli Save State ID.
        /// </summary>
        [Required]
        public string StateId { get; set; } = null!;

        /// <summary>
        /// Cannoli Save State.
        /// </summary>
        [ForeignKey(nameof(StateId))]
        public CannoliSaveState CannoliSaveState { get; set; } = null!;

        /// <summary>
        /// Indicates if the Cannoli Route should be processed synchronously.
        /// </summary>
        [Required]
        public bool IsSynchronous { get; set; }

        /// <summary>
        /// Indicates if the corresponding Discord interaction should be deferred upon executing the Cannoli Route.
        /// </summary>
        [Required]
        public bool IsDeferred { get; set; }

        /// <summary>
        /// Where applicable, indicates a Cannoli Save State ID that should be deleted upon executing the Cannoli Route.
        /// </summary>
        public string? StateIdToBeDeleted { get; set; } = null!;

        /// <summary>
        /// Generic string parameter 1, to be passed to the receiving callback.
        /// </summary>
        public string? Parameter1 { get; set; }

        /// <summary>
        /// Generic string parameter 2, to be passed to the receiving callback.
        /// </summary>
        public string? Parameter2 { get; set; }

        /// <summary>
        /// Generic string parameter 3, to be passed to the receiving callback.
        /// </summary>
        public string? Parameter3 { get; set; }

        [NotMapped]
        internal bool IsNew { get; set; }

        /// <summary>
        /// Gets the string representation of the Cannoli Route.
        /// </summary>
        /// <returns>The string representation of the Cannoli Route.</returns>
        public override string ToString()
        {
            return Id;
        }

        /// <summary>
        /// Implicitly converts a Cannoli Route to a string representation.
        /// </summary>
        /// <returns>The string representation of the Cannoli Route.</returns>
        public static implicit operator string(CannoliRoute route)
        {
            return route.ToString();
        }
    }
}
