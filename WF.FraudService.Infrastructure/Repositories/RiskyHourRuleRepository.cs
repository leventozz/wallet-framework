using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class RiskyHourRuleRepository(FraudDbContext _context, NpgsqlDataSource _dataSource) : IRiskyHourRuleRepository
{
    public async Task<RiskyHourRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "StartHour", "EndHour", "Description", "IsActive", "CreatedAtUtc"
            FROM "RiskyHourRules"
            WHERE "Id" = @id
            """;

        return await connection.QueryFirstOrDefaultAsync<RiskyHourRule>(
            new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<RiskyHourRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "StartHour", "EndHour", "Description", "IsActive", "CreatedAtUtc"
            FROM "RiskyHourRules"
            WHERE "IsActive" = true
            """;

        return await connection.QueryAsync<RiskyHourRule>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<RiskyHourRule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "StartHour", "EndHour", "Description", "IsActive", "CreatedAtUtc"
            FROM "RiskyHourRules"
            """;

        return await connection.QueryAsync<RiskyHourRule>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task AddAsync(RiskyHourRule rule, CancellationToken cancellationToken = default)
    {
        await _context.RiskyHourRules.AddAsync(rule, cancellationToken);
    }

    public async Task UpdateAsync(RiskyHourRule rule, CancellationToken cancellationToken = default)
    {
        _context.RiskyHourRules.Update(rule);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.RiskyHourRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is not null)
        {
            _context.RiskyHourRules.Remove(rule);
        }
    }
}

