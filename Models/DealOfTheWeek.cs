using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseOfWani.Models
{
    public class DealOfTheWeek
    {
        [Key]
        public int Id { get; set; } // Primary key
 

        public string Title { get; set; } // e.g. "Clothings Hot"
        public string Subtitle { get; set; } // e.g. "Shoe Collection"
        public string Description { get; set; } // e.g. "Accessories"

        public string? ImageUrl { get; set; } // e.g. "img/product-sale.png"


        [Column(TypeName = "decimal(6, 2)")]
        public decimal SalePrice { get; set; } // e.g. 29.99

        public string DealTitle { get; set; } // e.g. "Multi-pocket Chest Bag Black"
        public DateTime DealEndTime { get; set; }
        public bool IsActive { get; set; } // e.g. true
    }
}
