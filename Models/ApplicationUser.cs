using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models
{
    public class ApplicationUser : IdentityUser
    {

      
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public DateTime? LastLogin { get; set; }

    }
}
