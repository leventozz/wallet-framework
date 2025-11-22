using WF.FraudService.Application.Contracts.DTOs;

namespace WF.FraudService.Application.Contracts;

public interface IFraudRuleReadService
{
    Task<IEnumerable<BlockedIpRuleDto>> GetActiveBlockedIpsAsync(CancellationToken cancellationToken = default);
    Task<BlockedIpRuleDto?> GetBlockedIpRuleAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<RiskyHourRuleDto>> GetActiveRiskyHourRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountAgeRuleDto>> GetActiveAccountAgeRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<KycLevelRuleDto>> GetActiveKycLevelRulesAsync(CancellationToken cancellationToken = default);
}

