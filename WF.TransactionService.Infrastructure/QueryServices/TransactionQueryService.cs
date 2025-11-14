using Dapper;
using Npgsql;
using WF.TransactionService.Application.Contracts;
using WF.TransactionService.Application.Dtos;

namespace WF.TransactionService.Infrastructure.QueryServices
{
    public class TransactionQueryService(NpgsqlDataSource dataSource) : ITransactionReadService
    {
        public async Task<List<TransactionDto>> GetTransactionsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT 
                    "CorrelationId",
                    "CurrentState",
                    "SenderCustomerId",
                    "SenderCustomerNumber",
                    "ReceiverCustomerId",
                    "ReceiverCustomerNumber",
                    "SenderWalletId",
                    "SenderWalletNumber",
                    "ReceiverWalletId",
                    "ReceiverWalletNumber",
                    "Amount",
                    "Currency",
                    "CreatedAtUtc",
                    "CompletedAtUtc",
                    "FailureReason"
                FROM "TransferRequests"
                WHERE "SenderCustomerId" = @customerId OR "ReceiverCustomerId" = @customerId
                ORDER BY "CreatedAtUtc" DESC
                """;

            var transactions = await connection.QueryAsync<TransactionDto>(
                new CommandDefinition(sql, new { customerId }, cancellationToken: cancellationToken));

            return transactions.ToList();
        }
    }
}

