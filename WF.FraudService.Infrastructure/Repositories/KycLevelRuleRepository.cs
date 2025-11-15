using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.Enums;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class KycLevelRuleRepository(FraudDbContext _context, NpgsqlDataSource _dataSource) : IKycLevelRuleRepository
{
    public async Task<KycLevelRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "RequiredKycStatus", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "KycLevelRules"
            WHERE "Id" = @id
            """;

        return await connection.QueryFirstOrDefaultAsync<KycLevelRule>(
            new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<KycLevelRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "RequiredKycStatus", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "KycLevelRules"
            WHERE "IsActive" = true
            """;

        return await connection.QueryAsync<KycLevelRule>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<KycLevelRule>> GetByKycStatusAsync(KycStatus kycStatus, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "RequiredKycStatus", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "KycLevelRules"
            WHERE "RequiredKycStatus" = @kycStatus
            """;

        return await connection.QueryAsync<KycLevelRule>(
            new CommandDefinition(sql, new { kycStatus = (int)kycStatus }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<KycLevelRule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "RequiredKycStatus", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "KycLevelRules"
            """;

        return await connection.QueryAsync<KycLevelRule>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task AddAsync(KycLevelRule rule, CancellationToken cancellationToken = default)
    {
        await _context.KycLevelRules.AddAsync(rule, cancellationToken);
    }

    public async Task UpdateAsync(KycLevelRule rule, CancellationToken cancellationToken = default)
    {
        _context.KycLevelRules.Update(rule);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.KycLevelRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is not null)
        {
            _context.KycLevelRules.Remove(rule);
        }
    }
}

