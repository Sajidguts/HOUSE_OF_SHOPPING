using System.ComponentModel.DataAnnotations;

namespace HouseOfWani.Models
{
    public class CartViewModel
    {
       
        public List<CartItem> CartItems { get; set; }
        public List<CartItem> SavedForLaterItems { get; set; }
    }
}
