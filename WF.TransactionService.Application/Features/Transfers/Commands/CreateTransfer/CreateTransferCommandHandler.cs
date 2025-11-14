using MediatR;
using WF.Shared.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;

namespace WF.TransactionService.Application.Features.Transfers.Commands.CreateTransfer;

public class CreateTransferCommandHandler(
    IIntegrationEventPublisher _integrationEventPublisher,
    IUnitOfWork _unitOfWork)
    : IRequestHandler<CreateTransferCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var transferRequestStartedEvent = new TransferRequestStartedEvent
        {
            CorrelationId = correlationId,
            SenderCustomerId = request.SenderCustomerId,
            SenderCustomerNumber = request.SenderCustomerNumber,
            ReceiverCustomerId = request.ReceiverCustomerId,
            ReceiverCustomerNumber = request.ReceiverCustomerNumber,
            SenderWalletId = request.SenderWalletId,
            SenderWalletNumber = request.SenderWalletNumber,
            ReceiverWalletId = request.ReceiverWalletId,
            ReceiverWalletNumber = request.ReceiverWalletNumber,
            Amount = request.Amount,
            Currency = request.Currency
        };

        await _integrationEventPublisher.PublishAsync(transferRequestStartedEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return correlationId;
    }
}

