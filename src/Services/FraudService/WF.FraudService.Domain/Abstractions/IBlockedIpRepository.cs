using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IBlockedIpRepository
{
    Task AddAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default);
    Task UpdateAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

