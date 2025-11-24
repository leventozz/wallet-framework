using MassTransit;
using Microsoft.EntityFrameworkCore;
using WF.TransactionService.Domain.Entities;

namespace WF.TransactionService.Infrastructure.Data
{
    public class TransactionDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.CorrelationId);
                entity.Property(e => e.CurrentState).HasMaxLength(64);
                entity.Property(e => e.ClientIpAddress).HasMaxLength(45);
                entity.HasIndex(e => e.CurrentState).HasDatabaseName("IX_TransferRequests_CurrentState");
                entity.HasIndex(e => e.TransactionId).IsUnique().HasDatabaseName("IX_Transactions_TransactionId");
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

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
