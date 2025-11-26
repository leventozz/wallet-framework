using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IBlockedIpRepository
{
    Task AddAsync(BlockedIpRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(BlockedIpRule rule, CancellationToken cancellationToken = default);
    Task<BlockedIpRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
