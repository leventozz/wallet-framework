using WF.Shared.Contracts.Dtos;

namespace WF.Shared.Contracts.Abstractions
{
    public interface ICustomerServiceApiClient
    {
        Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
