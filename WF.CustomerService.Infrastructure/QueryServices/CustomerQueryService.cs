using Npgsql;
using WF.CustomerService.Application.Abstractions;
using Dapper;
using WF.Shared.Contracts.Dtos;

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

        public async Task<CustomerDto?> GetCustomerDtoByCustomerNoAsync(string customerNumber, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "CustomerNumber", "FirstName", "LastName", "Email", "PhoneNumber", "KycStatus", "CreatedAtUtc"
                FROM "Customers"
                WHERE "CustomerNumber" = @customerNumber AND "IsActive" = true AND "IsDeleted" = false;
                
                SELECT w."Id" AS "WalletId", w."Balance", w."Currency", w."State"
                FROM "WalletReadModels" w
                INNER JOIN "Customers" c ON w."CustomerId" = c."Id"
                WHERE c."CustomerNumber" = @customerNumber;
                """;

            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(sql, new { customerNumber }, cancellationToken: cancellationToken));

            var customer = await multi.ReadFirstOrDefaultAsync<CustomerDto>();

            if (customer == null)
            {
                return null;
            }

            var wallets = await multi.ReadAsync<WalletSummaryDto>();
            customer.Wallets = wallets.ToList();

            return customer;
        }

        public async Task<CustomerVerificationDto?> GetVerificationDataByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "Id", "CreatedAtUtc", "KycStatus"
                FROM "Customers"
                WHERE "Id" = @id AND "IsActive" = true AND "IsDeleted" = false;
                """;

            var verificationData = await connection.QueryFirstOrDefaultAsync<CustomerVerificationDto>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));

            return verificationData;
        }

        public async Task<Guid?> GetCustomerIdByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "Id"
                FROM "Customers"
                WHERE "CustomerNumber" = @customerNumber AND "IsActive" = true AND "IsDeleted" = false;
                """;

            var customerId = await connection.QueryFirstOrDefaultAsync<Guid?>(
                new CommandDefinition(sql, new { customerNumber }, cancellationToken: cancellationToken));

            return customerId;
        }

        public async Task<List<CustomerLookupDto>> LookupByCustomerNumbersAsync(List<string> customerNumbers, CancellationToken cancellationToken)
        {
            if (customerNumbers == null || customerNumbers.Count == 0)
            {
                return new List<CustomerLookupDto>();
            }

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "Id" AS "CustomerId", "CustomerNumber"
                FROM "Customers"
                WHERE "CustomerNumber" = ANY(@customerNumbers) AND "IsActive" = true AND "IsDeleted" = false;
                """;

            var results = await connection.QueryAsync<CustomerLookupDto>(
                new CommandDefinition(sql, new { customerNumbers }, cancellationToken: cancellationToken));

            return results.ToList();
        }
    }
}
