namespace ASM.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? ComboId { get; set; }
        public int Quantity { get; set; }
        public long UnitPrice { get; set; }

        public Order Order { get; set; } = null!;
        public Product? Product { get; set; }
        public Combo? Combo { get; set; }
    }
}
