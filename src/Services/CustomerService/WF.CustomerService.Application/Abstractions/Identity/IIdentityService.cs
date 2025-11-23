namespace WF.CustomerService.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<string> RegisterUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken);
}

