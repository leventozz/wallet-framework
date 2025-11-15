using WF.CustomerService.Domain.Entities;

namespace WF.CustomerService.Domain.Abstractions
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetCustomerByIdAsync(Guid customerId);
        Task AddCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(Guid customerId);
        Task UpdateCustomerAsync(Customer customer);
        Task<bool> IsCustomerNumberUniqueAsync(string customerNumber, CancellationToken cancellationToken);
    }
}
