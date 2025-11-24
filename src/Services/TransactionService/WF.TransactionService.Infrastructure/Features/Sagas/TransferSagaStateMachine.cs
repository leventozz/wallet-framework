using MassTransit;
using WF.Shared.Contracts.Commands.Fraud;
using WF.Shared.Contracts.Commands.Wallet;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.TransactionService.Domain.Entities;

namespace WF.TransactionService.Infrastructure.Features.Sagas;

public class TransferSagaStateMachine : MassTransitStateMachine<Transaction>
{
    public State Pending { get; private set; } = null!;
    public State FraudCheckApproved { get; private set; } = null!;
    public State SenderDebitPending { get; private set; } = null!;
    public State ReceiverCreditPending { get; private set; } = null!;
    public State SenderDebited { get; private set; } = null!;
    public State ReceiverCredited { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<TransferRequestStartedEvent> TransferRequestStarted { get; private set; } = null!;
    public Event<FraudCheckApprovedEvent> FraudCheckApprovedEvent { get; private set; } = null!;
    public Event<FraudCheckDeclinedEvent> FraudCheckDeclinedEvent { get; private set; } = null!;
    public Event<WalletDebitedEvent> WalletDebitedEvent { get; private set; } = null!;
    public Event<WalletDebitFailedEvent> WalletDebitFailedEvent { get; private set; } = null!;
    public Event<WalletCreditedEvent> WalletCreditedEvent { get; private set; } = null!;
    public Event<WalletCreditFailedEvent> WalletCreditFailedEvent { get; private set; } = null!;

    public TransferSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TransferRequestStarted, e => e.CorrelateById(x => x.Message.CorrelationId));
        Event(() => FraudCheckApprovedEvent, e => e.CorrelateById(x => x.Message.CorrelationId));
        Event(() => FraudCheckDeclinedEvent, e => e.CorrelateById(x => x.Message.CorrelationId));
        Event(() => WalletDebitedEvent, e => e.CorrelateById(x => x.Message.CorrelationId));
        Event(() => WalletDebitFailedEvent, e => e.CorrelateById(x => x.Message.CorrelationId));
        Event(() => WalletCreditedEvent, e => e.CorrelateById(x => x.Message.CorrelationId));
        Event(() => WalletCreditFailedEvent, e => e.CorrelateById(x => x.Message.CorrelationId));

        Initially(
            When(TransferRequestStarted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.TransactionId = context.Message.TransactionId;
                    context.Saga.SenderCustomerId = context.Message.SenderCustomerId;
                    context.Saga.SenderCustomerNumber = context.Message.SenderCustomerNumber;
                    context.Saga.ReceiverCustomerId = context.Message.ReceiverCustomerId;
                    context.Saga.ReceiverCustomerNumber = context.Message.ReceiverCustomerNumber;
                    context.Saga.SenderWalletId = context.Message.SenderWalletId;
                    context.Saga.ReceiverWalletId = context.Message.ReceiverWalletId;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.Currency = context.Message.Currency;
                    context.Saga.CreatedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(Pending)
                .Publish(context => new CheckFraudCommandContract
                {
                    CorrelationId = context.Saga.CorrelationId,
                    SenderCustomerId = context.Saga.SenderCustomerId,
                    ReceiverCustomerId = context.Saga.ReceiverCustomerId,
                    Amount = context.Saga.Amount,
                    Currency = context.Saga.Currency
                })
        );

        During(Pending,
            When(FraudCheckApprovedEvent)
                .Publish(context => new DebitSenderWalletCommandContract
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OwnerCustomerId = context.Saga.SenderCustomerId,
                    Amount = context.Saga.Amount,
                    TransactionId = context.Saga.TransactionId
                })
                .TransitionTo(SenderDebitPending),

            When(FraudCheckDeclinedEvent)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .TransitionTo(Failed)
                .Finalize()
        );

        During(SenderDebitPending,
            When(WalletDebitedEvent)
                .Publish(context => new CreditWalletCommandContract
                {
                    CorrelationId = context.Saga.CorrelationId,
                    WalletId = context.Saga.ReceiverWalletId,
                    Amount = context.Saga.Amount,
                    Currency = context.Saga.Currency,
                    TransactionId = context.Saga.TransactionId
                })
                .TransitionTo(ReceiverCreditPending),

            When(WalletDebitFailedEvent)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .TransitionTo(Failed)
                .Finalize()
        );

        During(ReceiverCreditPending,
            When(WalletCreditedEvent)
                .Then(context =>
                {
                    context.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(WalletCreditFailedEvent)
                .Publish(context => new RefundSenderWalletCommandContract
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OwnerCustomerId = context.Saga.SenderCustomerId,
                    Amount = context.Saga.Amount,
                    TransactionId = context.Saga.TransactionId
                })
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .TransitionTo(Failed)
                .Finalize()
        );
    }
}

