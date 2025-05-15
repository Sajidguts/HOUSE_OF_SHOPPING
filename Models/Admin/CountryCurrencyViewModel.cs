using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models.Admin
{
    public class CountryCurrencyViewModel
    {
        [Key]
        public int Id { get; set; }
        public Country? Country { get; set; }
        public Currency? Currency { get; set; }
    }
}
