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
                WHERE "Id" = @id AND "IsActive" = true AND "IsDeleted" = false
                """;

            var customer = await connection.QueryFirstOrDefaultAsync<CustomerDto>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));

            return customer;
        }
    }
}
