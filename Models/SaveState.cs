using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CannoliKit.Models
{
    [Index(nameof(ExpiresOn))]
    public sealed class SaveState
    {
        [Key]
        public string Id { get; set; } = null!;

        [Required]
        public byte[] State { get; set; } = null!;

        [Required]
        public DateTime UpdatedOn { get; set; }

        public DateTime? ExpiresOn { get; set; }
    }
}
