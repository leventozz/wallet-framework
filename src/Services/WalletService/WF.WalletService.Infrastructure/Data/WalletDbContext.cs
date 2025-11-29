using MassTransit;
using Microsoft.EntityFrameworkCore;
using WF.WalletService.Domain.Entities;
using WF.WalletService.Domain.ValueObjects;

namespace WF.WalletService.Infrastructure.Data
{
    public class WalletDbContext : DbContext
    {
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

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

                entity.Property<uint>("xmin")
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();

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

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).HasMaxLength(20);
                entity.Property(e => e.TableName).HasMaxLength(128);
                entity.Property(e => e.PrimaryKey).HasMaxLength(256);
                entity.Property(e => e.UserId).HasMaxLength(256);
                entity.HasIndex(e => e.TableName).HasDatabaseName("IX_AuditLogs_TableName");
                entity.HasIndex(e => e.DateTimeUtc).HasDatabaseName("IX_AuditLogs_DateTimeUtc");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AuditLogs_UserId");
            });
        }
    }
}

