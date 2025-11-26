using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.WalletService.Application.Features.Admin.Commands.FreezeWallet;

public record FreezeWalletCommand(Guid WalletId) : IRequest<Result>;
