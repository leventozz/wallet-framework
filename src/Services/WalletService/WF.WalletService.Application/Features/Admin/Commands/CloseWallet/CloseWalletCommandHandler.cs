using MediatR;
using WF.Shared.Contracts.Result;
using WF.Shared.Contracts.Abstractions;
using WF.WalletService.Domain.Abstractions;

namespace WF.WalletService.Application.Features.Admin.Commands.CloseWallet;

public class CloseWalletCommandHandler(IWalletRepository _walletRepository, IUnitOfWork _unitOfWork) : IRequestHandler<CloseWalletCommand, Result>
{
    public async Task<Result> Handle(CloseWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetWalletByIdForUpdateAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure(Error.NotFound("Wallet.NotFound", $"Wallet with ID {request.WalletId} was not found."));
        }

        var result = wallet.Close();
        if (result.IsFailure)
        {
            return result;
        }

        await _walletRepository.UpdateWalletAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
