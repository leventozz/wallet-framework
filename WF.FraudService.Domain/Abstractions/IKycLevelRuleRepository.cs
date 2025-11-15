using WF.FraudService.Domain.Entities;
using WF.FraudService.Domain.Enums;

namespace WF.FraudService.Domain.Abstractions;

public interface IKycLevelRuleRepository
{
    Task<KycLevelRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<KycLevelRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<KycLevelRule>> GetByKycStatusAsync(KycStatus kycStatus, CancellationToken cancellationToken = default);
    Task<IEnumerable<KycLevelRule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(KycLevelRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(KycLevelRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

