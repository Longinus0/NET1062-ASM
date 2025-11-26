namespace ASM.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public long Price { get; set; } // VND
        public string? Image { get; set; }
        public bool IsAvailable { get; set; } = true;

        public Category Category { get; set; } = null!;
    }
}
