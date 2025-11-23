using Npgsql;
using WF.CustomerService.Application.Abstractions;
using Dapper;
using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Infrastructure.QueryServices
{
    public class CustomerQueryService(NpgsqlDataSource dataSource) : ICustomerQueryService
    {
        public async Task<CustomerLookupDto?> GetCustomerByIdentityAsync(string identityId, CancellationToken cancellationToken)
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            const string sql = """
                SELECT "Id" AS "CustomerId", "CustomerNumber"
                FROM "Customers"
                WHERE "IdentityId" = @identityId AND "IsActive" = true AND "IsDeleted" = false;
                """;

            var result = await connection.QueryFirstOrDefaultAsync<CustomerLookupDto>(
                new CommandDefinition(sql, new { identityId }, cancellationToken: cancellationToken));

            return result;
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
