using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models
{
    public class GroceryOrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GroceryOrderId { get; init; }

        [Required]
        public int FoodItemId { get; init; }

        [ForeignKey(nameof(GroceryOrderId))]
        public GroceryOrder GroceryOrder { get; init; } = null!;

        [ForeignKey(nameof(FoodItemId))]
        public FoodItem FoodItem { get; init; } = null!;
    }
}
