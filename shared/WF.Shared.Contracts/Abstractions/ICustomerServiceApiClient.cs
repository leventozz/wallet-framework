using WF.Shared.Contracts.Dtos;

namespace WF.Shared.Contracts.Abstractions
{
    public interface ICustomerServiceApiClient
    {
        Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken);
        Task<CustomerLookupDto?> GetCustomerByIdentityAsync(string identityId, CancellationToken cancellationToken);
        Task<CustomerVerificationDto?> GetVerificationDataAsync(Guid customerId, CancellationToken cancellationToken);
        Task<List<CustomerLookupDto>> LookupByCustomerNumbersAsync(List<string> customerNumbers, CancellationToken cancellationToken);
    }
}
