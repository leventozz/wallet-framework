using MediatR;
using System.Text.Json.Serialization;

namespace WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;

public record CreateTransactionCommand : IRequest<Guid>
{
    public string SenderIdentityId { get; set; } = string.Empty;
    [JsonIgnore]
    public string SenderCustomerNumber { get; init; } = string.Empty;
    public string ReceiverCustomerNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

