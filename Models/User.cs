namespace ASM.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? PasswordHash { get; set; }
        public string? GoogleId { get; set; }
        public string Role { get; set; } = "customer"; // customer | admin
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
