using MediatR;
using WF.Shared.Contracts.Result;
using WF.Shared.Contracts.Abstractions;
using WF.WalletService.Domain.Abstractions;

namespace WF.WalletService.Application.Features.Admin.Commands.CloseWallet;

public class CloseWalletCommandHandler(IWalletRepository _walletRepository, IUnitOfWork _unitOfWork) : IRequestHandler<CloseWalletCommand, Result>
{
    public async Task<Result> Handle(CloseWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetWalletByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure(Error.NotFound("Wallet", request.WalletId));
        }

        var result = wallet.Close();
        if (result.IsFailure)
        {
            return result;
        }


        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
