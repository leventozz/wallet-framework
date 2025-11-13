using MassTransit;
using Microsoft.EntityFrameworkCore;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Infrastructure.Data.ReadModels;

namespace WF.CustomerService.Infrastructure.Data
{
    public class CustomerDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<WalletReadModel> WalletReadModels { get; set; } = null!;
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
            modelBuilder.AddInboxStateEntity(); //kind of idempotency

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasQueryFilter(c => c.IsActive && !c.IsDeleted);

                entity.HasIndex(c => c.CustomerNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Customers_CustomerNumber");
            });

            modelBuilder.Entity<WalletReadModel>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.HasIndex(w => w.CustomerId)
                    .HasDatabaseName("IX_WalletReadModels_CustomerId");
            });
        }
    }
}
