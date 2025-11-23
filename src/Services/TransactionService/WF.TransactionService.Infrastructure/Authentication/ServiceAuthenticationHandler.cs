using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace WF.TransactionService.Infrastructure.Authentication;

public class ServiceAuthenticationHandler(
    ITokenCacheService tokenCacheService,
    ILogger<ServiceAuthenticationHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await tokenCacheService.GetTokenAsync(cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                logger.LogDebug("Service authentication token added to outgoing request {RequestUri}", request.RequestUri);
            }
            else
            {
                logger.LogWarning("No service authentication token available for request {RequestUri}", request.RequestUri);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add service authentication token to request {RequestUri}", request.RequestUri);

        }

        return await base.SendAsync(request, cancellationToken);
    }
}

