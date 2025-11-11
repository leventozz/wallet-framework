using MassTransit;
using Microsoft.EntityFrameworkCore;
using WF.WalletService.Domain.Entities;

namespace WF.WalletService.Infrastructure.Data
{
    public class WalletDbContext : DbContext
    {
        public DbSet<Wallet> Wallets { get; set; } = null!;

        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
            modelBuilder.AddInboxStateEntity();

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasIndex(w => w.WalletNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Wallets_WalletNumber");

                entity.HasIndex(w => w.CustomerId)
                    .HasDatabaseName("IX_Wallets_CustomerId");
            });
        }
    }
}

