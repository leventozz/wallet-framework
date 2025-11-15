using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IRiskyHourRuleRepository
{
    Task<RiskyHourRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RiskyHourRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RiskyHourRule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(RiskyHourRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(RiskyHourRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

