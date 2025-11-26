using MediatR;
using WF.Shared.Contracts.Result;
using WF.Shared.Contracts.Abstractions;
using WF.WalletService.Domain.Abstractions;

namespace WF.WalletService.Application.Features.Admin.Commands.FreezeWallet;

public class FreezeWalletCommandHandler(IWalletRepository _walletRepository, IUnitOfWork _unitOfWork) : IRequestHandler<FreezeWalletCommand, Result>
{
    public async Task<Result> Handle(FreezeWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetWalletByIdForUpdateAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure(Error.NotFound("Wallet", request.WalletId));
        }

        var result = wallet.Freeze();
        if (result.IsFailure)
        {
            return result;
        }

        await _walletRepository.UpdateWalletAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
