using System.Security.Claims;
using WF.Shared.Contracts.Abstractions;
using WF.WalletService.Infrastructure.PropagationContext;

namespace WF.WalletService.Api.Services;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    UserContext userContext) : ICurrentUserService
{
    public string? UserId
    {
        get
        {

            var httpUserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            if (!string.IsNullOrEmpty(httpUserId))
            {
                return httpUserId;
            }


            return userContext.UserId;
        }
    }
}

