using Demo.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Helpers
{
    internal static class DataHelper
    {
        public static async Task InsertSampleData(DemoDbContext db)
        {
            var foodItems = new List<FoodItem>
            {
                new() {Emoji = "🍎", Name = "Apple"},
                new() {Emoji = "🍌", Name = "Banana"},
                new() {Emoji = "🍇", Name = "Grapes"},
                new() {Emoji = "🍉", Name = "Watermelon"},
                new() {Emoji = "🍓", Name = "Strawberry"},
                new() {Emoji = "🍒", Name = "Cherry"},
                new() {Emoji = "🍑", Name = "Peach"},
                new() {Emoji = "🍍", Name = "Pineapple"},
                new() {Emoji = "🥭", Name = "Mango"},
                new() {Emoji = "🍅", Name = "Tomato"},
                new() {Emoji = "🥕", Name = "Carrot"},
                new() {Emoji = "🌽", Name = "Corn"},
                new() {Emoji = "🥔", Name = "Potato"},
                new() {Emoji = "🍠", Name = "Sweet Potato"},
                new() {Emoji = "🥒", Name = "Cucumber"},
                new() {Emoji = "🥬", Name = "Lettuce"},
                new() {Emoji = "🍞", Name = "Bread"},
                new() {Emoji = "🧀", Name = "Cheese"},
                new() {Emoji = "🍗", Name = "Chicken Leg"},
                new() {Emoji = "🍔", Name = "Hamburger"},
                new() {Emoji = "🍕", Name = "Pizza"},
                new() {Emoji = "🍟", Name = "French Fries"},
                new() {Emoji = "🌭", Name = "Hot Dog"},
                new() {Emoji = "🍿", Name = "Popcorn"}
            };

            var existingFoodItems = await db.FoodItems.ToListAsync();

            foreach (var foodItem in foodItems)
            {
                if (existingFoodItems.Any(x => x.Name == foodItem.Name)) continue;

                db.FoodItems.Add(foodItem);
            }
        }
    }
}
