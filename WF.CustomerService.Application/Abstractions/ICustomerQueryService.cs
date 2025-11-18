using WF.Shared.Contracts.Dtos;

namespace WF.CustomerService.Application.Abstractions
{
    public interface ICustomerQueryService
    {
        Task<CustomerDto?> GetCustomerDtoByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<CustomerDto?> GetCustomerDtoByCustomerNoAsync(string customerNumber, CancellationToken cancellationToken);
        Task<CustomerVerificationDto?> GetVerificationDataByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Guid?> GetCustomerIdByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken);
    }
}
