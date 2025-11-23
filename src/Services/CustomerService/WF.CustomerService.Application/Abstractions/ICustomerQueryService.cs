using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Abstractions
{
    public interface ICustomerQueryService
    {
        Task<CustomerLookupDto?> GetCustomerByIdentityAsync(string identityId, CancellationToken cancellationToken);
        Task<CustomerVerificationDto?> GetVerificationDataByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<CustomerLookupDto>> LookupByCustomerNumbersAsync(List<string> customerNumbers, CancellationToken cancellationToken);
    }
}
