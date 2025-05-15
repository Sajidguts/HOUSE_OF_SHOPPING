using HouseOfWani.Models.Admin;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseOfWani.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; } // Navigation property
        public int ProductId { get; set; }
        //public string? userId { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public string SizeName { get; set; }
        public Product Product { get; set; } // Navigation property
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(6, 2)")]
        public decimal Price { get; set; }
       // public bool IsSavedForLater { get; set; } = false;


        [NotMapped]
        public decimal Subtotal => Quantity * Price;

        [Column(TypeName = "decimal(6, 2)")]
        public decimal TotalPrice { get; set; }

        // public string UserId { get; set; }
        //public ApplicationUser User { get; set; }
        //public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
