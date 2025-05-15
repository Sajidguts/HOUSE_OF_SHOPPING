
using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
    }
}
