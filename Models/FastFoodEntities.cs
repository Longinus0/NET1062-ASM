using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; } // Can be null for old data

        [Column("last_name")]
        public string? LastName { get; set; } // Can be null for old data

        [Column("email")]
        public string? Email { get; set; } // FIX: Made nullable to handle Admin user

        [Column("phone")]
        public string Phone { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("google_id")]
        public string? GoogleId { get; set; }

        [Column("role")]
        public string Role { get; set; } = "customer";

        [Column("avatar")]
        public string? Avatar { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("address_id")]
        public int? DefaultAddressId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("DefaultAddressId")]
        public virtual Address? DefaultAddress { get; set; }
        
        [InverseProperty("User")] 
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    [Table("Addresses")]
    public class Address
    {
        [Key]
        [Column("address_id")]
        public int AddressId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("recipient_name")]
        public string RecipientName { get; set; } = null!;

        [Column("phone")]
        public string Phone { get; set; } = null!;

        [Column("street")]
        public string Street { get; set; } = null!;

        [Column("ward")]
        public string? Ward { get; set; }

        [Column("district")]
        public string District { get; set; } = null!;

        [Column("city")]
        public string City { get; set; } = "Hồ Chí Minh";

        [Column("is_default")]
        public int IsDefault { get; set; } = 0;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

    [Table("Categories")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("image")]
        public string? Image { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    [Table("Products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Column("is_available")]
        public int IsAvailable { get; set; } = 1;

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
    }

    [Table("Combos")]
    public class Combo
    {
        [Key]
        [Column("combo_id")]
        public int ComboId { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Column("is_available")]
        public int IsAvailable { get; set; } = 1;

        public virtual ICollection<ComboDetail> ComboDetails { get; set; } = new List<ComboDetail>();
    }

    [Table("ComboDetails")]
    public class ComboDetail
    {
        [Column("combo_id")]
        public int ComboId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo Combo { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }

    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("guest_name")]
        public string? GuestName { get; set; }

        [Column("guest_phone")]
        public string? GuestPhone { get; set; }

        [Column("address_id")]
        public int? AddressId { get; set; }

        [Column("full_address")]
        public string FullAddress { get; set; } = null!;

        [Column("payment_method")]
        public string PaymentMethod { get; set; } = null!;

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("note")]
        public string? Note { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual User? User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual Payment? Payment { get; set; }
    }

    [Table("OrderDetails")]
    public class OrderDetail
    {
        [Key]
        [Column("order_detail_id")]
        public int OrderDetailId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("product_id")]
        public int? ProductId { get; set; }

        [Column("combo_id")]
        public int? ComboId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
        
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }
    }

    [Table("Payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("method")]
        public string Method { get; set; } = null!;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("transaction_id")]
        public string? TransactionId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}
