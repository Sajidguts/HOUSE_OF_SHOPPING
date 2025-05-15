using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models.Admin
{
    public class ProductColor
    {
        [Key]
        public int Id { get; set; }  // Add a primary key field if necessary
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int ColorId { get; set; }
        public Color Color { get; set; }
    }
}
