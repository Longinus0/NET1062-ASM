namespace ASM.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int? UserId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string FullAddress { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public long TotalAmount { get; set; }
        public string Status { get; set; } = "pending";
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
