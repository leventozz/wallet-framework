using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class BlockedIpRuleRepository(FraudDbContext _context, NpgsqlDataSource _dataSource) : IBlockedIpRepository
{
    public async Task<BlockedIpRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "IpAddress", "Reason", "CreatedAtUtc", "ExpiresAtUtc", "IsActive"
            FROM "BlockedIps"
            WHERE "Id" = @id
            """;

        return await connection.QueryFirstOrDefaultAsync<BlockedIpRule>(
            new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
    }

    public async Task<BlockedIpRule?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT "Id", "IpAddress", "Reason", "CreatedAtUtc", "ExpiresAtUtc", "IsActive"
            FROM "BlockedIps"
            WHERE "IpAddress" = @ipAddress
            """;

        return await connection.QueryFirstOrDefaultAsync<BlockedIpRule>(
            new CommandDefinition(sql, new { ipAddress }, cancellationToken: cancellationToken));
    }

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

    public async Task AddAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default)
    {
        await _context.BlockedIps.AddAsync(blockedIp, cancellationToken);
    }

    public async Task UpdateAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default)
    {
        _context.BlockedIps.Update(blockedIp);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blockedIp = await _context.BlockedIps
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (blockedIp is not null)
        {
            _context.BlockedIps.Remove(blockedIp);
        }
    }
}

