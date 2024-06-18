using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class GroceryOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public DateTime OrderedOn { get; set; }

        [Required]
        public bool IsFulfilled { get; set; }

        public IList<GroceryOrderItem> Items { get; set; } = [];
    }
}
