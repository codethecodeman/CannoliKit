using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class FoodItem
    {
        [Key]
        public int Id { get; init; }

        [Required]
        public string Name { get; init; } = null!;

        [Required]
        public string Emoji { get; init; } = null!;
    }
}
