using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IAccountAgeRuleRepository
{
    Task AddAsync(AccountAgeRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountAgeRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

