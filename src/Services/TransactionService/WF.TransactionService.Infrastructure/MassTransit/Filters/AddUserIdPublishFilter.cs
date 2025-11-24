using MassTransit;
using WF.Shared.Contracts.Abstractions;

namespace WF.TransactionService.Infrastructure.MassTransit.Filters;


public class AddUserIdPublishFilter<T>(ICurrentUserService currentUserService) : IFilter<PublishContext<T>>
    where T : class
{
    public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
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
        context.CreateFilterScope("AddUserIdPublishFilter");
    }
}

