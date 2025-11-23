namespace WF.TransactionService.Infrastructure.Authentication
{
    public interface IKeycloakTokenService
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
        Task<TokenResult> GetTokenAsync(CancellationToken cancellationToken = default);
    }
}
