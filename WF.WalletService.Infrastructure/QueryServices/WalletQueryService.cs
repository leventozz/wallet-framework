using Dapper;
using Npgsql;
using WF.Shared.Contracts.Dtos;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Application.Dtos;

namespace WF.WalletService.Infrastructure.QueryServices
{
    public class WalletQueryService(NpgsqlDataSource dataSource) : IWalletQueryService
    {
        public async Task<WalletDto?> GetWalletByOwnerIdAsync(Guid ownerCustomerId, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT 
                    "Id",
                    "CustomerId",
                    "WalletNumber",
                    "Currency"::text AS "Currency",
                    "Balance",
                    "AvailableBalance",
                    "IsActive",
                    "IsFrozen",
                    "IsClosed",
                    "CreatedAtUtc",
                    "UpdatedAtUtc",
                    "ClosedAtUtc",
                    "LastTransactionId",
                    "LastTransactionAtUtc",
                    "Iban",
                    "ExternalAccountRef"
                FROM "Wallets"
                WHERE "CustomerId" = @ownerCustomerId AND "IsDeleted" = false
                """;

            var wallet = await connection.QueryFirstOrDefaultAsync<WalletDto>(
                new CommandDefinition(sql, new { ownerCustomerId }, cancellationToken: cancellationToken));

            return wallet;
        }

        public async Task<Guid?> GetWalletIdByCustomerIdAndCurrencyAsync(Guid customerId, string currency, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "Id"
                FROM "Wallets"
                WHERE "CustomerId" = @customerId 
                    AND "Currency"::text = @currency 
                    AND "IsDeleted" = false 
                    AND "IsActive" = true 
                    AND "IsClosed" = false
                    AND "IsFrozen" = false
                LIMIT 1
                """;

            var walletId = await connection.QueryFirstOrDefaultAsync<Guid?>(
                new CommandDefinition(sql, new { customerId, currency }, cancellationToken: cancellationToken));

            return walletId;
        }

        public async Task<List<WalletLookupDto>> LookupByCustomerIdsAsync(List<Guid> customerIds, string currency, CancellationToken cancellationToken)
        {
            if (customerIds == null || customerIds.Count == 0)
            {
                return new List<WalletLookupDto>();
            }

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "CustomerId", "Id" AS "WalletId"
                FROM "Wallets"
                WHERE "CustomerId" = ANY(@customerIds) 
                    AND "Currency"::text = @currency 
                    AND "IsDeleted" = false 
                    AND "IsActive" = true 
                    AND "IsClosed" = false
                    AND "IsFrozen" = false;
                """;

            var results = await connection.QueryAsync<WalletLookupDto>(
                new CommandDefinition(sql, new { customerIds, currency }, cancellationToken: cancellationToken));

            return results.ToList();
        }
    }
}
