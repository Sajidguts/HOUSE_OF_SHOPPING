using Microsoft.EntityFrameworkCore;
using HouseOfWani.Models.Admin;

namespace HouseOfWani.Models.Order
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
            public Product Product { get; set; }
        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal PriceAtPurchase { get; set; }

        public Order Order { get; set; }
    }
}
