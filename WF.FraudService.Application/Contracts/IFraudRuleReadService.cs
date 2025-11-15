using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Application.Contracts;

public interface IFraudRuleReadService
{
    Task<IEnumerable<BlockedIpRule>> GetActiveBlockedIpsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsIpBlockedAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<RiskyHourRule>> GetActiveRiskyHourRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountAgeRule>> GetActiveAccountAgeRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<KycLevelRule>> GetActiveKycLevelRulesAsync(CancellationToken cancellationToken = default);
}

