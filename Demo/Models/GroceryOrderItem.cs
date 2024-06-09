using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models
{
    [PrimaryKey(nameof(GroceryOrderId), nameof(FoodItemId))]
    public class GroceryOrderItem
    {
        public int GroceryOrderId { get; init; }
        public int FoodItemId { get; init; }

        [ForeignKey(nameof(GroceryOrderId))]
        public GroceryOrder GroceryOrder { get; init; } = null!;

        [ForeignKey(nameof(FoodItemId))]
        public FoodItem FoodItem { get; init; } = null!;
    }
}
