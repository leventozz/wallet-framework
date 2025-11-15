using Dapper;
using Npgsql;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Infrastructure.QueryServices;

public class FraudRuleReadService(NpgsqlDataSource _dataSource) : IFraudRuleReadService
{
    public async Task<IEnumerable<BlockedIpRule>> GetActiveBlockedIpsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "IpAddress", "Reason", "CreatedAtUtc", "ExpiresAtUtc", "IsActive"
            FROM "BlockedIps"
            WHERE "IsActive" = true 
                AND ("ExpiresAtUtc" IS NULL OR "ExpiresAtUtc" > @now)
            """;

        return await connection.QueryAsync<BlockedIpRule>(
            new CommandDefinition(sql, new { now = DateTime.UtcNow }, cancellationToken: cancellationToken));
    }

    public async Task<bool> IsIpBlockedAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(1)
            FROM "BlockedIps"
            WHERE "IpAddress" = @ipAddress 
                AND "IsActive" = true 
                AND ("ExpiresAtUtc" IS NULL OR "ExpiresAtUtc" > @now)
            """;

        var count = await connection.QuerySingleAsync<int>(
            new CommandDefinition(sql, new { ipAddress, now = DateTime.UtcNow }, cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<IEnumerable<RiskyHourRule>> GetActiveRiskyHourRulesAsync(CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<AccountAgeRule>> GetActiveAccountAgeRulesAsync(CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<KycLevelRule>> GetActiveKycLevelRulesAsync(CancellationToken cancellationToken = default)
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
}

