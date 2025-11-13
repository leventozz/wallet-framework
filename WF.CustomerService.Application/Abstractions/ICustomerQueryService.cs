using WF.CustomerService.Application.Dtos;

namespace WF.CustomerService.Application.Abstractions
{
    public interface ICustomerQueryService
    {
        Task<CustomerDto?> GetCustomerDtoByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<CustomerDto?> GetCustomerDtoByCustomerNoAsync(string customerNumber, CancellationToken cancellationToken);
    }
}
