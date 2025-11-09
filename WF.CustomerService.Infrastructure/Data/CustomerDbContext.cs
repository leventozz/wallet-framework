using Microsoft.EntityFrameworkCore;
using WF.CustomerService.Domain.Entities;

namespace WF.CustomerService.Infrastructure.Data
{
    public class CustomerDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; } = null!;
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
            
        }
    }
}
