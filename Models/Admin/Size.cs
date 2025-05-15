using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models.Admin
{
    public class Size
    {

        [Key]
        public int Id { get; set; }
        public string Label { get; set; }

        public ICollection<ProductSize> ProductSizes { get; set; }
    }
}
