using MassTransit;
using Microsoft.EntityFrameworkCore;
using WF.TransactionService.Domain.Entities;

namespace WF.TransactionService.Infrastructure.Data
{
    public class TransactionDbContext : DbContext
    {
        public DbSet<Transaction> TransferRequests { get; set; } = null!;

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
                entity.HasIndex(e => e.CurrentState).HasDatabaseName("IX_TransferRequests_CurrentState");
            });


            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
