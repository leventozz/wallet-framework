using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class AccountAgeRuleRepository(FraudDbContext _context, NpgsqlDataSource _dataSource) : IAccountAgeRuleRepository
{
    public async Task<AccountAgeRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "MinAccountAgeDays", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "AccountAgeRules"
            WHERE "Id" = @id
            """;

        return await connection.QueryFirstOrDefaultAsync<AccountAgeRule>(
            new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<AccountAgeRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "MinAccountAgeDays", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "AccountAgeRules"
            WHERE "IsActive" = true
            """;

        return await connection.QueryAsync<AccountAgeRule>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<AccountAgeRule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "MinAccountAgeDays", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "AccountAgeRules"
            """;

        return await connection.QueryAsync<AccountAgeRule>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task AddAsync(AccountAgeRule rule, CancellationToken cancellationToken = default)
    {
        await _context.AccountAgeRules.AddAsync(rule, cancellationToken);
    }

    public async Task UpdateAsync(AccountAgeRule rule, CancellationToken cancellationToken = default)
    {
        _context.AccountAgeRules.Update(rule);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.AccountAgeRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is not null)
        {
            _context.AccountAgeRules.Remove(rule);
        }
    }
}

