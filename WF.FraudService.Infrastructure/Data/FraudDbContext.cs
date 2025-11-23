using MassTransit;
using Microsoft.EntityFrameworkCore;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.ValueObjects;

namespace WF.FraudService.Infrastructure.Data;

public class FraudDbContext : DbContext
{
    public DbSet<BlockedIpRule> BlockedIps { get; set; } = null!;
    public DbSet<RiskyHourRule> RiskyHourRules { get; set; } = null!;
    public DbSet<AccountAgeRule> AccountAgeRules { get; set; } = null!;
    public DbSet<KycLevelRule> KycLevelRules { get; set; } = null!;

    public FraudDbContext(DbContextOptions<FraudDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();

        ConfigureBlockedIp(modelBuilder);
        ConfigureRiskyHourRule(modelBuilder);
        ConfigureAccountAgeRule(modelBuilder);
        ConfigureKycLevelRule(modelBuilder);
    }

    private static void ConfigureBlockedIp(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlockedIpRule>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.IpAddress)
                .HasDatabaseName("IX_BlockedIps_IpAddress");

            entity.HasIndex(e => new { e.IpAddress, e.IsActive })
                .HasDatabaseName("IX_BlockedIps_IpAddress_IsActive");

            entity.Property(e => e.IpAddress)
                .HasConversion(
                    ip => ip.ToString(),
                    str => IpAddress.FromDatabaseValue(str))
                .IsRequired()
                .HasMaxLength(45);

            entity.Property(e => e.Reason)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired();
        });
    }

    private static void ConfigureRiskyHourRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RiskyHourRule>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_RiskyHourRules_IsActive");

            entity.ComplexProperty(e => e.TimeRange, timeRange =>
            {
                timeRange.Property(t => t.StartHour)
                    .HasColumnName("StartHour")
                    .IsRequired();

                timeRange.Property(t => t.EndHour)
                    .HasColumnName("EndHour")
                    .IsRequired();
            });

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .IsRequired();

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();
        });
    }

    private static void ConfigureAccountAgeRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountAgeRule>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_AccountAgeRules_IsActive");

            entity.Property(e => e.MinAccountAgeDays)
                .IsRequired();

            entity.Property(e => e.MaxAllowedAmount)
                .HasConversion(
                    money => money.HasValue ? money.Value.Amount : (decimal?)null,
                    dec => dec.HasValue ? Money.FromDatabaseValue(dec.Value) : (Money?)null)
                .HasPrecision(18, 2);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .IsRequired();

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();
        });
    }

    private static void ConfigureKycLevelRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KycLevelRule>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_KycLevelRules_IsActive");

            entity.HasIndex(e => e.RequiredKycStatus)
                .HasDatabaseName("IX_KycLevelRules_RequiredKycStatus");

            entity.Property(e => e.RequiredKycStatus)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.MaxAllowedAmount)
                .HasConversion(
                    money => money.HasValue ? money.Value.Amount : (decimal?)null,
                    dec => dec.HasValue ? Money.FromDatabaseValue(dec.Value) : (Money?)null)
                .HasPrecision(18, 2);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .IsRequired();

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();
        });
    }
}

