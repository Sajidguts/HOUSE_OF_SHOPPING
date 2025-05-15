using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models
{
    public class BlogPost
    {
        public int Id { get; set; } // Primary key
        public string Title { get; set; }
        public string? Slug { get; set; } // for SEO-friendly URLs
        public string ImageUrl { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; } // full blog content
        public string ReadMoreUrl { get; set; }
    }
}
