using WF.FraudService.Application.Contracts.DTOs;

namespace WF.FraudService.Application.Contracts;

public interface IAdminFraudRuleQueryService
{
    Task<IEnumerable<AccountAgeRuleDto>> GetAllAccountAgeRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BlockedIpRuleDto>> GetAllBlockedIpRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<KycLevelRuleDto>> GetAllKycLevelRulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RiskyHourRuleDto>> GetAllRiskyHourRulesAsync(CancellationToken cancellationToken = default);
}
