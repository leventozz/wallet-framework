using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IBlockedIpRepository
{
    Task<BlockedIpRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BlockedIpRule?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlockedIpRule>> GetActiveBlockedIpsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsIpBlockedAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task AddAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default);
    Task UpdateAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

