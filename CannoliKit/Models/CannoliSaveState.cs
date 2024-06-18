using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CannoliKit.Models
{
    /// <summary>
    /// Represents a Cannoli Save State. Handles state information for a Cannoli Module.
    /// </summary>
    [Index(nameof(ExpiresOn))]
    public sealed class CannoliSaveState
    {
        /// <summary>
        /// Cannoli Save State ID.
        /// </summary>
        [Key]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Serialized state information.
        /// </summary>
        [Required]
        public byte[] State { get; set; } = null!;

        /// <summary>
        /// UTC datetime of last state update.
        /// </summary>
        [Required]
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// UTC datetime of state expiration.
        /// </summary>
        public DateTime? ExpiresOn { get; set; }
    }
}
