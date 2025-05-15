using Microsoft.EntityFrameworkCore;// Correct namespace for IdentityDbContext
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HouseOfWani.Models.Admin;
namespace HouseOfWani.Models.Order
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Country> Countries { get; set; }


        public DbSet<DealOfTheWeek> DealOfTheWeeks { get; set; }
        public DbSet<InstagramPost> InstagramPosts { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }

        public DbSet<BannerImage> BannerImages { get; set; }
        public DbSet<Currency> Currencies { get; set; } 
        public DbSet<Category> Categories { get; set; }
       // public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        //   public ICollection<ProductColor> ProductColors { get; set; }
        //public ICollection<ProductSize> ProductSizes { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductDetail> ProductDetails { get; set; }
        //  public ICollection<ProductSize> ProductSizes { get; set; }
        //  public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
         public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

    

    }


}