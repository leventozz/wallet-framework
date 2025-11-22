using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.ValueObjects;
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

                entity.Property(c => c.Email)
                    .HasConversion(
                        email => email.Value,
                        value => Email.FromDatabaseValue(value))
                    .HasColumnName("Email")
                    .HasMaxLength(320);

                entity.Property(c => c.PhoneNumber)
                    .HasConversion(
                        phoneNumber => phoneNumber.Value,
                        value => PhoneNumber.FromDatabaseValue(value))
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20);

                entity.ComplexProperty(c => c.Name, nameBuilder =>
                {
                    nameBuilder.Property(n => n.FirstName)
                        .HasColumnName("FirstName")
                        .HasMaxLength(100)
                        .IsRequired();

                    nameBuilder.Property(n => n.LastName)
                        .HasColumnName("LastName")
                        .HasMaxLength(100)
                        .IsRequired();
                });
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
