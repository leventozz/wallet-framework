using MassTransit;
using WF.TransactionService.Infrastructure.Context;

namespace WF.TransactionService.Infrastructure.MassTransit.Filters;


public class ExtractUserIdConsumeFilter<T>(UserContext userContext) : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        string? userId = null;
        if (context.Headers.TryGetHeader("X-User-Id", out var userIdObj) && userIdObj is string userIdValue)
        {
            userId = userIdValue;
        }

        using (userContext.SetUser(userId))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("ExtractUserIdConsumeFilter");
    }
}

