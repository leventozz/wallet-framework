using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WF.Shared.Contracts.Abstractions;

namespace WF.TransactionService.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
}

