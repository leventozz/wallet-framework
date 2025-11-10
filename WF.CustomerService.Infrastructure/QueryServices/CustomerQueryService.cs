using Mapster;
using Microsoft.EntityFrameworkCore;
using WF.CustomerService.Application.Abstractions;
using WF.CustomerService.Application.Dtos;
using WF.CustomerService.Infrastructure.Data;

namespace WF.CustomerService.Infrastructure.QueryServices
{
    public class CustomerQueryService : ICustomerQueryService
    {
        private readonly CustomerDbContext _dbContext;

        public CustomerQueryService(CustomerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CustomerDto?> GetCustomerDtoByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Customers
                .AsNoTracking()
                .Where(c => c.Id == id)
                .ProjectToType<CustomerDto>()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
