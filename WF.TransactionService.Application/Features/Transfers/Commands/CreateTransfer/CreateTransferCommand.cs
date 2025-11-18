using MediatR;

namespace WF.TransactionService.Application.Features.Transfers.Commands.CreateTransfer;

public record CreateTransferCommand : IRequest<Guid>
{
    public string SenderCustomerNumber { get; init; } = string.Empty;
    public string ReceiverCustomerNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

