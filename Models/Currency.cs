using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HouseOfWani.Models
{
    public class Currency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        [Column(TypeName = "decimal(6, 2)")]
        public decimal ConversionRateToBase { get; set; }
    }
}
