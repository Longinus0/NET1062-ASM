using Microsoft.EntityFrameworkCore;
using ASM.Models;

namespace ASM.Data
{
    public class FastFoodDbContext : DbContext
    {
        public FastFoodDbContext(DbContextOptions<FastFoodDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Combo> Combos { get; set; } = null!;
        public DbSet<ComboDetail> ComboDetails { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ComboDetail>()
                .HasKey(cd => new { cd.ComboId, cd.ProductId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
