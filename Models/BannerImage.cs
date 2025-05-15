using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models
{
    public class BannerImage
    {

            [Key]
            public int Id { get; set; }  // Primary key for the banner image

            [Required]  // Ensures that the field is not null
            [StringLength(255)]  // Max length for the URL
            public string SummerImage1 { get; set; }  // URL of the image file
         

            [StringLength(255)]
            public string AltText { get; set; }  // Text to describe the image for accessibility (SEO, screen readers)

            public string Title { get; set; }  // Title for the banner, e.g., "Summer Sale!"

            public string Description { get; set; }  // Description or promotional text

            public string ? ButtonText { get; set; }  // Text for the call-to-action button (e.g., "Shop Now")

            [Url]  // Validates that the URL is a valid web address
            public string ButtonLink { get; set; }  // URL the button will link to when clicked

            public bool IsActive { get; set; }  // Flag to determine if the banner is active or not (show/hide)

            public DateTime CreatedAt { get; set; }  // Timestamp of when the banner was created

            public DateTime? ExpiryDate { get; set; }  // Optional expiry date to control when the banner should be hidden

            public int DisplayOrder { get; set; }  // Optional field to determine order in which banners are displayed
        }

    }

