using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models
{
    public class InstagramPost
    {
        [Key]
        public int Id { get; set; } // Primary key
        public string ImageUrl { get; set; }
    }
}
