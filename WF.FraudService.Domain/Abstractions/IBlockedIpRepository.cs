using WF.FraudService.Domain.Entities;

namespace WF.FraudService.Domain.Abstractions;

public interface IBlockedIpRepository
{
    Task<BlockedIp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BlockedIp?> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlockedIp>> GetActiveBlockedIpsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsIpBlockedAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task AddAsync(BlockedIp blockedIp, CancellationToken cancellationToken = default);
    Task UpdateAsync(BlockedIp blockedIp, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

