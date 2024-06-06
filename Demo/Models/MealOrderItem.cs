using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models
{
    [PrimaryKey(nameof(MealOrderId), nameof(FoodItemId))]
    public class MealOrderItem
    {
        public int MealOrderId { get; init; }
        public int FoodItemId { get; init; }

        [ForeignKey(nameof(MealOrderId))]
        public MealOrder MealOrder { get; init; } = null!;

        [ForeignKey(nameof(FoodItemId))]
        public FoodItem FoodItem { get; init; } = null!;
    }
}
