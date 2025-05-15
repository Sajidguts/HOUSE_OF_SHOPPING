namespace HouseOfWani.Models.Admin
{
    public class Color
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<ProductColor> ProductColors { get; set; }
    }
}
