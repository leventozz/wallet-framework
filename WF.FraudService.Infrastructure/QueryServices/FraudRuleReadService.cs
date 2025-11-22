using Dapper;
using Npgsql;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;

namespace WF.FraudService.Infrastructure.QueryServices;

public class FraudRuleReadService(NpgsqlDataSource _dataSource) : IFraudRuleReadService
{
    public async Task<IEnumerable<BlockedIpRuleDto>> GetActiveBlockedIpsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "IpAddress", "Reason", "CreatedAtUtc", "ExpiresAtUtc", "IsActive"
            FROM "BlockedIps"
            WHERE "IsActive" = true 
                AND ("ExpiresAtUtc" IS NULL OR "ExpiresAtUtc" > @now)
            """;

        return await connection.QueryAsync<BlockedIpRuleDto>(
            new CommandDefinition(sql, new { now = DateTime.UtcNow }, cancellationToken: cancellationToken));
    }

    public async Task<BlockedIpRuleDto?> GetBlockedIpRuleAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "IpAddress", "Reason", "CreatedAtUtc", "ExpiresAtUtc", "IsActive"
            FROM "BlockedIps"
            WHERE "IpAddress" = @ipAddress 
                AND "IsActive" = true
            LIMIT 1
            """;

        return await connection.QueryFirstOrDefaultAsync<BlockedIpRuleDto>(
            new CommandDefinition(sql, new { ipAddress }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<RiskyHourRuleDto>> GetActiveRiskyHourRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "StartHour", "EndHour", "Description", "IsActive", "CreatedAtUtc"
            FROM "RiskyHourRules"
            WHERE "IsActive" = true
            """;

        return await connection.QueryAsync<RiskyHourRuleDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<AccountAgeRuleDto>> GetActiveAccountAgeRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "MinAccountAgeDays", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "AccountAgeRules"
            WHERE "IsActive" = true
            """;

        return await connection.QueryAsync<AccountAgeRuleDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<KycLevelRuleDto>> GetActiveKycLevelRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "RequiredKycStatus", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "KycLevelRules"
            WHERE "IsActive" = true
            """;

        return await connection.QueryAsync<KycLevelRuleDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}

