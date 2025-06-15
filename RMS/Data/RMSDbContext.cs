using Microsoft.EntityFrameworkCore;
using RMS.Data.Entities;
using RMS.Data.Entities.Base;

namespace RMS.Data
{
    public class RMSDbContext : DbContext
    {
        public RMSDbContext(DbContextOptions<RMSDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillDiscount> BillDiscounts { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishIngredient> DishIngredients { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Stock> Stocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Additional configuration if needed
        }

        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.Now;
                }
            }
        }

    }
}
