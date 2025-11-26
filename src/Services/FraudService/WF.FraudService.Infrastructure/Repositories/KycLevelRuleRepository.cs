using Microsoft.EntityFrameworkCore;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class KycLevelRuleRepository(FraudDbContext _context) : IKycLevelRuleRepository
{
    public async Task AddAsync(KycLevelRule rule, CancellationToken cancellationToken = default)
    {
        await _context.KycLevelRules.AddAsync(rule, cancellationToken);
    }

    public async Task UpdateAsync(KycLevelRule rule, CancellationToken cancellationToken = default)
    {
        _context.KycLevelRules.Update(rule);
        await Task.CompletedTask;
    }

    public async Task<KycLevelRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.KycLevelRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.KycLevelRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule is not null)
        {
            _context.KycLevelRules.Remove(rule);
        }
    }
}

