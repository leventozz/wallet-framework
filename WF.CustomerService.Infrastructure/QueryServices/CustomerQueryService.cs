using Npgsql;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Application.Dtos;
using Dapper;

namespace WF.CustomerService.Infrastructure.QueryServices
{
    public class CustomerQueryService(NpgsqlDataSource dataSource) : ICustomerQueryService
    {
        public async Task<CustomerDto?> GetCustomerDtoByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "CustomerNumber", "FirstName", "LastName", "Email", "PhoneNumber", "KycStatus", "CreatedAtUtc"
                FROM "Customers"
                WHERE "Id" = @id AND "IsActive" = true AND "IsDeleted" = false;
                
                SELECT "Id" AS "WalletId", "Balance", "Currency", "State"
                FROM "WalletReadModels"
                WHERE "CustomerId" = @id;
                """;

            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));

            var customer = await multi.ReadFirstOrDefaultAsync<CustomerDto>();

            if (customer == null)
            {
                return null;
            }

            var wallets = await multi.ReadAsync<WalletSummaryDto>();
            customer.Wallets = wallets.ToList();

            return customer;
        }
    }
}
