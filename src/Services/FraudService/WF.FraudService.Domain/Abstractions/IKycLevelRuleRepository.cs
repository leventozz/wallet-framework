using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IKycLevelRuleRepository
{
    Task AddAsync(KycLevelRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(KycLevelRule rule, CancellationToken cancellationToken = default);
    Task<KycLevelRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

