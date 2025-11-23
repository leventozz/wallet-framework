using Microsoft.EntityFrameworkCore;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class AccountAgeRuleRepository(FraudDbContext _context) : IAccountAgeRuleRepository
{
    public async Task AddAsync(AccountAgeRule rule, CancellationToken cancellationToken = default)
    {
        await _context.AccountAgeRules.AddAsync(rule, cancellationToken);
    }

    public async Task UpdateAsync(AccountAgeRule rule, CancellationToken cancellationToken = default)
    {
        _context.AccountAgeRules.Update(rule);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.AccountAgeRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is not null)
        {
            _context.AccountAgeRules.Remove(rule);
        }
    }
}

