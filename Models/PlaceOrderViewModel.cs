namespace HouseOfWani.Models
{
    public class PlaceOrderViewModel
    {
        public int cardId { get; set; }
        public List<CartItem> CartItems { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalDAmount { get; set; }
    }
}
