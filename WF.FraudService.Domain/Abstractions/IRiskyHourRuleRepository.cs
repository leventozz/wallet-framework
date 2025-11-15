using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IRiskyHourRuleRepository
{
    Task AddAsync(RiskyHourRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(RiskyHourRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

