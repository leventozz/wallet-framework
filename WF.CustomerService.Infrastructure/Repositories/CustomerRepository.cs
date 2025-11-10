using Microsoft.EntityFrameworkCore;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.Repositories;
using WF.CustomerService.Infrastructure.Data;

namespace WF.CustomerService.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerDbContext _context;

        public CustomerRepository(CustomerDbContext context)
        {
            _context = context;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public async Task DeleteCustomerAsync(Guid customerId)
        {
            var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer is not null)
            {
                _context.Customers.Remove(customer);
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
        {
            return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
        }

        public async Task<bool> IsCustomerNumberUniqueAsync(string customerNumber, CancellationToken cancellationToken)
        {
            return !await _context.Customers
                .AsNoTracking()
                .AnyAsync(c => c.CustomerNumber == customerNumber, cancellationToken);
        }
    }
}
