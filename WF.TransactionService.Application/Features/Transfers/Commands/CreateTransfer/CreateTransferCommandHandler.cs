using MediatR;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.TransactionService.Domain.Exceptions;

namespace WF.TransactionService.Application.Features.Transfers.Commands.CreateTransfer;

public class CreateTransferCommandHandler(
    IIntegrationEventPublisher _integrationEventPublisher,
    IUnitOfWork _unitOfWork,
    ICustomerServiceApiClient _customerServiceApiClient,
    IWalletServiceApiClient _walletServiceApiClient)
    : IRequestHandler<CreateTransferCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        // Get customer IDs
        var senderCustomerId = await _customerServiceApiClient.GetCustomerIdByCustomerNumberAsync(
            request.SenderCustomerNumber, 
            cancellationToken);

        if (senderCustomerId == null)
        {
            throw new NotFoundException("Customer", request.SenderCustomerNumber);
        }

        var receiverCustomerId = await _customerServiceApiClient.GetCustomerIdByCustomerNumberAsync(
            request.ReceiverCustomerNumber, 
            cancellationToken);

        if (receiverCustomerId == null)
        {
            throw new NotFoundException("Customer", request.ReceiverCustomerNumber);
        }

        // Get wallet IDs
        var senderWalletId = await _walletServiceApiClient.GetWalletIdByCustomerIdAndCurrencyAsync(
            senderCustomerId.Value, 
            request.Currency, 
            cancellationToken);

        if (senderWalletId == null)
        {
            throw new NotFoundException($"Wallet for customer {senderCustomerId.Value} with currency {request.Currency}");
        }

        var receiverWalletId = await _walletServiceApiClient.GetWalletIdByCustomerIdAndCurrencyAsync(
            receiverCustomerId.Value, 
            request.Currency, 
            cancellationToken);

        if (receiverWalletId == null)
        {
            throw new NotFoundException($"Wallet for customer {receiverCustomerId.Value} with currency {request.Currency}");
        }

        var transferRequestStartedEvent = new TransferRequestStartedEvent
        {
            CorrelationId = correlationId,
            SenderCustomerId = senderCustomerId.Value,
            SenderCustomerNumber = request.SenderCustomerNumber,
            ReceiverCustomerId = receiverCustomerId.Value,
            ReceiverCustomerNumber = request.ReceiverCustomerNumber,
            SenderWalletId = senderWalletId.Value,
            SenderWalletNumber = request.SenderWalletNumber,
            ReceiverWalletId = receiverWalletId.Value,
            ReceiverWalletNumber = request.ReceiverWalletNumber,
            Amount = request.Amount,
            Currency = request.Currency
        };

        await _integrationEventPublisher.PublishAsync(transferRequestStartedEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return correlationId;
    }
}

