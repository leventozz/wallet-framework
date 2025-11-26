using Dapper;
using Microsoft.EntityFrameworkCore;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Application.Dtos;
using WF.WalletService.Application.Dtos.Filters;
using WF.WalletService.Infrastructure.Data;

namespace WF.WalletService.Infrastructure.QueryServices;

public class AdminWalletQueryService(WalletDbContext _dbContext) : IAdminWalletQueryService
{
    public async Task<PagedResult<AdminWalletListDto>> GetWalletsAsync(WalletListFilter filter, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var sql = @"
            SELECT 
                ""Id"",
                ""CustomerId"",
                ""WalletNumber"",
                ""Currency"",
                ""Balance"",
                ""AvailableBalance"",
                ""IsActive"",
                ""IsFrozen"",
                ""IsClosed"",
                ""CreatedAtUtc"",
                ""UpdatedAtUtc"",
                ""Iban"",
                ""LastTransactionId"",
                ""LastTransactionAtUtc""
            FROM ""Wallets""
            WHERE ""IsDeleted"" = false";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.WalletNumber))
        {
            sql += " AND \"WalletNumber\" = @WalletNumber";
            parameters.Add("WalletNumber", filter.WalletNumber);
        }

        if (!string.IsNullOrWhiteSpace(filter.Currency))
        {
            sql += " AND \"Currency\" = @Currency";
            parameters.Add("Currency", filter.Currency);
        }

        if (filter.IsActive.HasValue)
        {
            sql += " AND \"IsActive\" = @IsActive";
            parameters.Add("IsActive", filter.IsActive.Value);
        }

        if (filter.IsFrozen.HasValue)
        {
            sql += " AND \"IsFrozen\" = @IsFrozen";
            parameters.Add("IsFrozen", filter.IsFrozen.Value);
        }

        if (filter.IsClosed.HasValue)
        {
            sql += " AND \"IsClosed\" = @IsClosed";
            parameters.Add("IsClosed", filter.IsClosed.Value);
        }

        if (filter.StartDate.HasValue)
        {
            sql += " AND \"CreatedAtUtc\" >= @StartDate";
            parameters.Add("StartDate", filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            sql += " AND \"CreatedAtUtc\" <= @EndDate";
            parameters.Add("EndDate", filter.EndDate.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM ({sql}) AS CountQuery";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        sql += " ORDER BY \"CreatedAtUtc\" DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        parameters.Add("Offset", (filter.PageNumber - 1) * filter.PageSize);
        parameters.Add("PageSize", filter.PageSize);

        var wallets = await connection.QueryAsync<AdminWalletListDto>(sql, parameters);

        return new PagedResult<AdminWalletListDto>(
            wallets.ToList(),
            filter.PageNumber,
            filter.PageSize,
            totalCount
        );
    }
}
