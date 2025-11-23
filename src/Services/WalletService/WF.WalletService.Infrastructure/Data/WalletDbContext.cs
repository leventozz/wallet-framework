using MassTransit;
using Microsoft.EntityFrameworkCore;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.ValueObjects;

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

                entity.ComplexProperty(w => w.Balance, balanceBuilder =>
                {
                    balanceBuilder.Property(b => b.Amount)
                        .HasColumnName("Balance")
                        .HasPrecision(18, 2)
                        .IsRequired();

                    balanceBuilder.Property(b => b.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(10)
                        .IsRequired();
                });

                entity.ComplexProperty(w => w.AvailableBalance, availableBalanceBuilder =>
                {
                    availableBalanceBuilder.Property(ab => ab.Amount)
                        .HasColumnName("AvailableBalance")
                        .HasPrecision(18, 2)
                        .IsRequired();

                    availableBalanceBuilder.Property(ab => ab.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(10)
                        .IsRequired();
                });

                entity.Property(w => w.Iban)
                    .HasConversion(
                        iban => iban.HasValue ? iban.Value.Value : null,
                        value => !string.IsNullOrWhiteSpace(value) ? Iban.FromDatabaseValue(value) : (Iban?)null)
                    .HasColumnName("Iban")
                    .HasMaxLength(34);
            });
        }
    }
}

