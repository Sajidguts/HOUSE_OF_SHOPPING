using System.ComponentModel.DataAnnotations.Schema;

namespace HouseOfWani.Models
{
    public class Cart
    {


        public int Id { get; set; }
        public string UserId { get; set; } // Optional for guest carts
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
        [Column(TypeName = "decimal(6, 2)")]
        [NotMapped]
        public decimal TotalPrice => Items.Sum(item => item.Price);

        public decimal DiscountedPrice => Items.Sum(item => item.Product.DiscountedPrice);

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
      
    }
}
