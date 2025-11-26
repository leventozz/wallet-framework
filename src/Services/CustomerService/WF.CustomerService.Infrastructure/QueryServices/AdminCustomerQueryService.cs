using Dapper;
using Npgsql;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Application.Features.Customers.Queries.GetAllCustomersWithWallets;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Infrastructure.QueryServices;

public class AdminCustomerQueryService(NpgsqlDataSource dataSource) : IAdminCustomerQueryService
{
    public async Task<PagedResult<AdminCustomerListDto>> GetAllCustomersWithWalletsAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var offset = (pageNumber - 1) * pageSize;

        const string countSql = """
            SELECT COUNT(*)
            FROM "Customers"
            WHERE "IsDeleted" = false;
            """;

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, cancellationToken: cancellationToken));

        const string sql = """
            SELECT 
                c."Id",
                c."CustomerNumber",
                CONCAT(c."FirstName", ' ', c."LastName") AS "FullName",
                c."Email",
                c."IsActive",
                c."CreatedAtUtc" AS "CreatedAt",
                w."Id" AS "WalletId",
                w."WalletNumber",
                w."Balance",
                w."Currency",
                w."State"
            FROM "Customers" c
            LEFT JOIN "WalletReadModels" w ON c."Id" = w."CustomerId"
            WHERE c."IsDeleted" = false
            ORDER BY c."CreatedAtUtc" DESC
            OFFSET @offset LIMIT @pageSize;
            """;

        var customerDictionary = new Dictionary<Guid, AdminCustomerListDto>();

        await connection.QueryAsync<AdminCustomerListDto, AdminWalletDto?, AdminCustomerListDto>(
            new CommandDefinition(sql, new { offset, pageSize }, cancellationToken: cancellationToken),
            (customer, wallet) =>
            {
                if (!customerDictionary.TryGetValue(customer.Id, out var existingCustomer))
                {
                    existingCustomer = customer;
                    customerDictionary.Add(customer.Id, existingCustomer);
                }

                if (wallet != null)
                {
                    existingCustomer.Wallets.Add(wallet);
                }

                return existingCustomer;
            },
            splitOn: "WalletId");

        var customers = customerDictionary.Values.ToList();

        return PagedResult<AdminCustomerListDto>.Create(customers, totalCount, pageNumber, pageSize);
    }
}
