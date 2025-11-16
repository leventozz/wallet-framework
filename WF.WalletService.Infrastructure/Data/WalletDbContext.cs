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
            modelBuilder.AddInboxStateEntity(); //idempontecy

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasQueryFilter(w => !w.IsDeleted);

                entity.HasIndex(w => w.WalletNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Wallets_WalletNumber");

                entity.HasIndex(w => w.CustomerId)
                    .HasDatabaseName("IX_Wallets_CustomerId");

                entity.Property(w => w.Balance)
                    .HasPrecision(18, 2);

                entity.Property(w => w.AvailableBalance)
                    .HasPrecision(18, 2);
            });
        }
    }
}

