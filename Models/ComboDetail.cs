namespace ASM.Models
{
    public class ComboDetail
    {
        public int ComboId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;

        public Combo Combo { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
