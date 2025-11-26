namespace ASM.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Image { get; set; }
        public int DisplayOrder { get; set; } = 0;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
