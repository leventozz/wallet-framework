using MediatR;
using System.Text.Json.Serialization;

using WF.Shared.Contracts.Result;

namespace WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;

public record CreateTransactionCommand : IRequest<Result<string>>
{
    public string SenderIdentityId { get; set; } = string.Empty;
    [JsonIgnore]
    public string SenderCustomerNumber { get; init; } = string.Empty;
    public string ReceiverCustomerNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    [JsonIgnore]
    public string? ClientIpAddress { get; init; }
}

