using WF.Shared.Contracts.Dtos;

namespace WF.Shared.Contracts.Abstractions
{
    public interface ICustomerServiceApiClient
    {
        Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken);
        Task<CustomerVerificationDto?> GetVerificationDataAsync(Guid customerId, CancellationToken cancellationToken);
        Task<Guid?> GetCustomerIdByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken);
        Task<List<CustomerLookupDto>> LookupByCustomerNumbersAsync(List<string> customerNumbers, CancellationToken cancellationToken);
    }
}
