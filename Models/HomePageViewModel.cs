using HouseOfWani.Models.Admin;

namespace HouseOfWani.Models
{
    public class HomePageViewModel
    {
        public List<DealOfTheWeek>? DealOfTheWeeks { get; set; }
        public List<InstagramPost>? InstagramPosts { get; set; }
        public List<BlogPost>? BlogPosts { get; set; }
        public List<BannerImage>? BannerImages { get; set; }
        public List<Product>? Products { get; set; }
    }
}
