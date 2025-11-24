using MassTransit;
using WF.Shared.Contracts.Abstractions;
using WF.TransactionService.Infrastructure.Context;

namespace WF.TransactionService.Infrastructure.MassTransit.Filters;


public class AddUserIdSendFilter<T>(ICurrentUserService currentUserService) : IFilter<SendContext<T>>
    where T : class
{
    public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        var userId = currentUserService.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            //ecrypt header for prod
            context.Headers.Set("X-User-Id", userId);
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("AddUserIdSendFilter");
    }
}

