using Microsoft.EntityFrameworkCore;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class RiskyHourRuleRepository(FraudDbContext _context) : IRiskyHourRuleRepository
{
    public async Task AddAsync(RiskyHourRule rule, CancellationToken cancellationToken = default)
    {
        await _context.RiskyHourRules.AddAsync(rule, cancellationToken);
    }

    public async Task UpdateAsync(RiskyHourRule rule, CancellationToken cancellationToken = default)
    {
        _context.RiskyHourRules.Update(rule);
        await Task.CompletedTask;
    }

    public async Task<RiskyHourRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RiskyHourRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.RiskyHourRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is not null)
        {
            _context.RiskyHourRules.Remove(rule);
        }
    }
}

