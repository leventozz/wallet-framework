using Dapper;
using Microsoft.EntityFrameworkCore;
using WF.Shared.Contracts.Result;
using WF.TransactionService.Application.Contracts;
using WF.TransactionService.Application.Dtos;
using WF.TransactionService.Application.Dtos.Filters;
using WF.TransactionService.Infrastructure.Data;

namespace WF.TransactionService.Infrastructure.QueryServices;

public class AdminTransactionQueryService(TransactionDbContext _dbContext) : IAdminTransactionQueryService
{
    public async Task<PagedResult<AdminTransactionListDto>> GetTransactionsAsync(TransactionListFilter filter, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var sql = @"
            SELECT 
                ""CorrelationId"",
                ""TransactionId"",
                ""CurrentState"",
                ""SenderCustomerNumber"",
                ""ReceiverCustomerNumber"",
                ""Amount"",
                ""Currency"",
                ""CreatedAtUtc"",
                ""CompletedAtUtc"",
                ""FailureReason"",
                ""ClientIpAddress""
            FROM ""Transactions""
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (filter.CorrelationId.HasValue)
        {
            sql += " AND \"CorrelationId\" = @CorrelationId";
            parameters.Add("CorrelationId", filter.CorrelationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.TransactionId))
        {
            sql += " AND \"TransactionId\" = @TransactionId";
            parameters.Add("TransactionId", filter.TransactionId);
        }

        if (!string.IsNullOrWhiteSpace(filter.CurrentState))
        {
            sql += " AND \"CurrentState\" = @CurrentState";
            parameters.Add("CurrentState", filter.CurrentState);
        }

        if (!string.IsNullOrWhiteSpace(filter.SenderCustomerNumber))
        {
            sql += " AND \"SenderCustomerNumber\" = @SenderCustomerNumber";
            parameters.Add("SenderCustomerNumber", filter.SenderCustomerNumber);
        }

        if (!string.IsNullOrWhiteSpace(filter.ReceiverCustomerNumber))
        {
            sql += " AND \"ReceiverCustomerNumber\" = @ReceiverCustomerNumber";
            parameters.Add("ReceiverCustomerNumber", filter.ReceiverCustomerNumber);
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

        var transactions = await connection.QueryAsync<AdminTransactionListDto>(sql, parameters);

        return new PagedResult<AdminTransactionListDto>(
            transactions.ToList(),
            filter.PageNumber,
            filter.PageSize,
            totalCount
        );
    }
}
