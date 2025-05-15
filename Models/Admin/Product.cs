using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseOfWani.Models.Admin
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Column(TypeName = "decimal(6, 2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(6, 2)")]
        public decimal DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = "";
        
        public int Qauntity { get; set; }

        public int CountryId { get; set; }
      
       
        public int CurrencyId { get; set; }
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
        //     public ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
        public ICollection<ProductDetail> productdetails { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public bool IsActive { get; set; } = true;

    }

}

