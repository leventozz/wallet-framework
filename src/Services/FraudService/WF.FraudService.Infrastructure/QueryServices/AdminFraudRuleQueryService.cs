using Dapper;
using Npgsql;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Contracts.DTOs;

namespace WF.FraudService.Infrastructure.QueryServices;

public class AdminFraudRuleQueryService(NpgsqlDataSource _dataSource) : IAdminFraudRuleQueryService
{
    public async Task<IEnumerable<AccountAgeRuleDto>> GetAllAccountAgeRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "MinAccountAgeDays", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "AccountAgeRules"
            WHERE "IsActive" = true
            ORDER BY "CreatedAtUtc" DESC
            """;

        return await connection.QueryAsync<AccountAgeRuleDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<BlockedIpRuleDto>> GetAllBlockedIpRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "IpAddress", "Reason", "CreatedAtUtc", "ExpiresAtUtc", "IsActive"
            FROM "BlockedIps"
            WHERE "IsActive" = true
            ORDER BY "CreatedAtUtc" DESC
            """;

        return await connection.QueryAsync<BlockedIpRuleDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<KycLevelRuleDto>> GetAllKycLevelRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "RequiredKycStatus", "MaxAllowedAmount", "Description", "IsActive", "CreatedAtUtc"
            FROM "KycLevelRules"
            WHERE "IsActive" = true
            ORDER BY "CreatedAtUtc" DESC
            """;

        return await connection.QueryAsync<KycLevelRuleDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<RiskyHourRuleDto>> GetAllRiskyHourRulesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "StartHour", "EndHour", "Description", "IsActive", "CreatedAtUtc"
            FROM "RiskyHourRules"
            WHERE "IsActive" = true
            ORDER BY "CreatedAtUtc" DESC
            """;

        return await connection.QueryAsync<RiskyHourRuleDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
