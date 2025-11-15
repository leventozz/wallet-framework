using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IAccountAgeRuleRepository
{
    Task<AccountAgeRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountAgeRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountAgeRule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(AccountAgeRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountAgeRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

