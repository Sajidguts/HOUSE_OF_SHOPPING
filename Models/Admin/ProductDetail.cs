using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models.Admin
{
    public class ProductDetail
    {
      
        [Key]
        public int Id { get; set; }
        public string? ImageUrl { get; set; }  // Image path or URL

        public int ProductId { get; set; }  // Foreign Key to Product

        public Product Product { get; set; }  // Navigation property
    }
}
