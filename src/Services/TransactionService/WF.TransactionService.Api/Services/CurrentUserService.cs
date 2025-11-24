using System.Security.Claims;
using WF.Shared.Contracts.Abstractions;

namespace WF.TransactionService.Api.Services;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    Infrastructure.Context.UserContext userContext) : ICurrentUserService
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

