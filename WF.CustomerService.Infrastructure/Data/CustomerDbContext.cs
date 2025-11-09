using Microsoft.EntityFrameworkCore;
using WF.CustomerService.Domain.Entities;

namespace WF.CustomerService.Infrastructure.Data
{
    public class CustomerDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; } = null!;
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasQueryFilter(c => c.IsActive && !c.IsDeleted);

                entity.HasIndex(c => c.CustomerNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Customers_CustomerNumber");
            });
        }
    }
}
