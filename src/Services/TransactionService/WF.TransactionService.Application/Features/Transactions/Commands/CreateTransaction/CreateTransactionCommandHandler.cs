using IdGen;
using MediatR;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.TransactionService.Application.Abstractions;
using WF.Shared.Contracts.Result;

namespace WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler(
    IIntegrationEventPublisher _integrationEventPublisher,
    IUnitOfWork _unitOfWork,
    ICustomerServiceApiClient _customerServiceApiClient,
    IWalletServiceApiClient _walletServiceApiClient,
    IMachineContextProvider _machineContextProvider)
    : IRequestHandler<CreateTransactionCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();

        var machineId = _machineContextProvider.GetMachineId();
        var generator = new IdGenerator(machineId);
        var transactionId = $"TX-{generator.CreateId()}";

        var senderLookupTask = _customerServiceApiClient.GetCustomerByIdentityAsync(request.SenderIdentityId, cancellationToken);
        var receiverLookupTask = _customerServiceApiClient.LookupByCustomerNumbersAsync(
            new List<string> { request.ReceiverCustomerNumber },
            cancellationToken);

        await Task.WhenAll(senderLookupTask, receiverLookupTask);

        var senderCustomerLookup = await senderLookupTask;
        if (senderCustomerLookup == null)
        {
            return Result<string>.Failure(Error.NotFound("Sender customer", request.SenderIdentityId));
        }

        var receiverLookups = await receiverLookupTask;
        var receiverCustomerLookup = receiverLookups.FirstOrDefault(c => c.CustomerNumber == request.ReceiverCustomerNumber);
        
        if (receiverCustomerLookup == null)
        {
            return Result<string>.Failure(Error.NotFound("Receiver ustomer", request.ReceiverCustomerNumber));
        }

        var walletLookups = await _walletServiceApiClient.LookupByCustomerIdsAsync(
            new List<Guid> { senderCustomerLookup.CustomerId, receiverCustomerLookup.CustomerId },
            request.Currency,
            cancellationToken);

        var senderWalletLookup = walletLookups.FirstOrDefault(w => w.CustomerId == senderCustomerLookup.CustomerId);
        if (senderWalletLookup == null)
        {
            return Result<string>.Failure(Error.NotFound("Wallet", $"Sender customer {senderCustomerLookup.CustomerId} with currency {request.Currency}"));
        }

        var receiverWalletLookup = walletLookups.FirstOrDefault(w => w.CustomerId == receiverCustomerLookup.CustomerId);
        if (receiverWalletLookup == null)
        {
            return Result<string>.Failure(Error.NotFound("Wallet", $"Receiver customer {receiverCustomerLookup.CustomerId} with currency {request.Currency}"));
        }

        var transferRequestStartedEvent = new TransferRequestStartedEvent
        {
            CorrelationId = correlationId,
            TransactionId = transactionId,
            SenderCustomerId = senderCustomerLookup.CustomerId,
            SenderCustomerNumber = senderCustomerLookup.CustomerNumber,
            ReceiverCustomerId = receiverCustomerLookup.CustomerId,
            ReceiverCustomerNumber = request.ReceiverCustomerNumber,
            SenderWalletId = senderWalletLookup.WalletId,
            ReceiverWalletId = receiverWalletLookup.WalletId,
            Amount = request.Amount,
            Currency = request.Currency,
            ClientIpAddress = request.ClientIpAddress
        };

        await _integrationEventPublisher.PublishAsync(transferRequestStartedEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(transactionId);
    }
}
