using Microsoft.EntityFrameworkCore;
using WF.FraudService.Domain.Abstractions;
using WF.FraudService.Domain.Entities;
using WF.FraudService.Infrastructure.Data;

namespace WF.FraudService.Infrastructure.Repositories;

public class BlockedIpRuleRepository(FraudDbContext _context) : IBlockedIpRepository
{
    public async Task AddAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default)
    {
        await _context.BlockedIps.AddAsync(blockedIp, cancellationToken);
    }

    public async Task UpdateAsync(BlockedIpRule blockedIp, CancellationToken cancellationToken = default)
    {
        _context.BlockedIps.Update(blockedIp);
        await Task.CompletedTask;
    }

    public async Task<BlockedIpRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BlockedIps
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blockedIp = await _context.BlockedIps
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (blockedIp is not null)
        {
            _context.BlockedIps.Remove(blockedIp);
        }
    }
}

