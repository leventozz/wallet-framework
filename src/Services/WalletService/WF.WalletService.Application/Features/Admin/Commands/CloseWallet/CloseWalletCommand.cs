using MediatR;
using WF.Shared.Contracts.Result;

namespace WF.WalletService.Application.Features.Admin.Commands.CloseWallet;

public record CloseWalletCommand(Guid WalletId) : IRequest<Result>;
