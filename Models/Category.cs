using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } // Primary key
        [Required]
        public string Name { get; set; } // Name of the category (e.g., Men, Women)
        [Required]
        public int ProductCount { get; set; } // Count of products in the category
    }
}
