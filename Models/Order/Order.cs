using HouseOfWani.Models.Admin;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseOfWani.Models.Order
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }   // To know which user placed the order
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }
        [Column(TypeName = "decimal(6, 2)")]
        public decimal Tax { get; set; } = 0.00m;  // Default tax amount
        public string Status { get; set; } = "Confirmed";  // Auto-approve!
        [Column(TypeName = "decimal(6, 2)")]
        public decimal totalAfterTax { get; set; } = 0.00m; // Total amount after discount

        public string ShippingAddress { get; set; } = "";  // Shipping address for the order
        public List<OrderItem> OrderItems { get; set; }  // Products in the order
        public int cartId { get; set; } // Cart ID from which the order was placed
    }
}
