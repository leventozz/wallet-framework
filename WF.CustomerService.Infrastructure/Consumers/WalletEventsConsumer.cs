using MassTransit;
using Microsoft.Extensions.Logging;
using WF.CustomerService.Infrastructure.Data;
using WF.CustomerService.Infrastructure.Data.ReadModels;
using WF.Shared.Contracts.IntegrationEvents.Wallet;

namespace WF.CustomerService.Infrastructure.Consumers
{
    public class WalletEventsConsumer(
        CustomerDbContext context,
        ILogger<WalletEventsConsumer> logger)
        : IConsumer<WalletBalanceUpdatedEvent>, IConsumer<WalletCreatedEvent>, IConsumer<WalletStateChangedEvent>
    {

        //TODO: seperate consumers for each event
        public async Task Consume(ConsumeContext<WalletCreatedEvent> consumeContext)
        {
            var message = consumeContext.Message;

            var readModel = new WalletReadModel
            {
                Id = message.WalletId,
                CustomerId = message.CustomerId,
                Balance = message.InitialBalance,
                Currency = message.Currency
            };

            context.WalletReadModels.Add(readModel);
            await context.SaveChangesAsync();

            logger.LogInformation("Wallet read model created for WalletId {WalletId}, CustomerId {CustomerId}", message.WalletId, message.CustomerId);
        }

        public async Task Consume(ConsumeContext<WalletBalanceUpdatedEvent> consumeContext)
        {
            var message = consumeContext.Message;

            var wallet = await context.WalletReadModels.FindAsync(message.WalletId);

            if (wallet != null)
            {
                wallet.Balance = message.NewBalance;
                await context.SaveChangesAsync();

                logger.LogInformation("Wallet balance updated for WalletId {WalletId}, NewBalance {NewBalance}", message.WalletId, message.NewBalance);
            }
            else
            {
                logger.LogWarning("Wallet read model not found for WalletId {WalletId}", message.WalletId);
            }
        }

        public async Task Consume(ConsumeContext<WalletStateChangedEvent> consumeContext)
        {
            var message = consumeContext.Message;

            var wallet = await context.WalletReadModels.FindAsync(message.WalletId);

            if (wallet != null)
            {
                wallet.State = message.NewState;
                await context.SaveChangesAsync();

                logger.LogInformation("Wallet state updated for WalletId {WalletId}, PreviousState {PreviousState}, NewState {NewState}",
                    message.WalletId, message.PreviousState, message.NewState);
            }
            else
            {
                logger.LogWarning("Wallet read model not found for WalletId {WalletId}", message.WalletId);
            }
        }
    }
}
