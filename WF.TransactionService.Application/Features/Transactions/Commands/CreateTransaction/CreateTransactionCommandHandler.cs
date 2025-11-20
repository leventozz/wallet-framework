using MediatR;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.TransactionService.Domain.Exceptions;

namespace WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler(
    IIntegrationEventPublisher _integrationEventPublisher,
    IUnitOfWork _unitOfWork,
    ICustomerServiceApiClient _customerServiceApiClient,
    IWalletServiceApiClient _walletServiceApiClient)
    : IRequestHandler<CreateTransactionCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var senderLookupTask = _customerServiceApiClient.GetCustomerByIdentityAsync(request.SenderIdentityId, cancellationToken);
        var receiverLookupTask = _customerServiceApiClient.LookupByCustomerNumbersAsync(
            new List<string> { request.ReceiverCustomerNumber },
            cancellationToken);

        await Task.WhenAll(senderLookupTask, receiverLookupTask);

        var senderCustomerLookup = await senderLookupTask;
        if (senderCustomerLookup == null)
        {
            throw new NotFoundException("Customer", request.SenderIdentityId);
        }

        var receiverLookups = await receiverLookupTask;
        var receiverCustomerLookup = receiverLookups.FirstOrDefault(c => c.CustomerNumber == request.ReceiverCustomerNumber);
        
        if (receiverCustomerLookup == null)
        {
            throw new NotFoundException("Customer", request.ReceiverCustomerNumber);
        }

        var walletLookups = await _walletServiceApiClient.LookupByCustomerIdsAsync(
            new List<Guid> { senderCustomerLookup.CustomerId, receiverCustomerLookup.CustomerId },
            request.Currency,
            cancellationToken);

        var senderWalletLookup = walletLookups.FirstOrDefault(w => w.CustomerId == senderCustomerLookup.CustomerId);
        if (senderWalletLookup == null)
        {
            throw new NotFoundException("Wallet", $"Customer {senderCustomerLookup.CustomerId} with currency {request.Currency}");
        }

        var receiverWalletLookup = walletLookups.FirstOrDefault(w => w.CustomerId == receiverCustomerLookup.CustomerId);
        if (receiverWalletLookup == null)
        {
            throw new NotFoundException("Wallet", $"Customer {receiverCustomerLookup.CustomerId} with currency {request.Currency}");
        }

        var transferRequestStartedEvent = new TransferRequestStartedEvent
        {
            CorrelationId = correlationId,
            SenderCustomerId = senderCustomerLookup.CustomerId,
            SenderCustomerNumber = senderCustomerLookup.CustomerNumber,
            ReceiverCustomerId = receiverCustomerLookup.CustomerId,
            ReceiverCustomerNumber = request.ReceiverCustomerNumber,
            SenderWalletId = senderWalletLookup.WalletId,
            ReceiverWalletId = receiverWalletLookup.WalletId,
            Amount = request.Amount,
            Currency = request.Currency
        };

        await _integrationEventPublisher.PublishAsync(transferRequestStartedEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return correlationId;
    }
}
