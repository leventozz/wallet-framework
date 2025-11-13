using Dapper;
using Npgsql;
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
    }
}
