namespace ASM.Models
{
    public class Address
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string RecipientName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string? Ward { get; set; }
        public string District { get; set; } = null!;
        public string City { get; set; } = "Hồ Chí Minh";
        public bool IsDefault { get; set; } = false;

        public User User { get; set; } = null!;
    }
}
