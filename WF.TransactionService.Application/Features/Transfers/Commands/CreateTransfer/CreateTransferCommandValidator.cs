using FluentValidation;

namespace WF.TransactionService.Application.Features.Transfers.Commands.CreateTransfer;

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.SenderCustomerId)
            .NotEmpty()
            .WithMessage("Sender customer ID is required.");

        RuleFor(x => x.SenderCustomerNumber)
            .NotEmpty()
            .WithMessage("Sender customer number is required.");

        RuleFor(x => x.ReceiverCustomerId)
            .NotEmpty()
            .WithMessage("Receiver customer ID is required.");

        RuleFor(x => x.ReceiverCustomerNumber)
            .NotEmpty()
            .WithMessage("Receiver customer number is required.");

        RuleFor(x => x.SenderWalletId)
            .NotEmpty()
            .WithMessage("Sender wallet ID is required.");

        RuleFor(x => x.SenderWalletNumber)
            .NotEmpty()
            .WithMessage("Sender wallet number is required.");

        RuleFor(x => x.ReceiverWalletId)
            .NotEmpty()
            .WithMessage("Receiver wallet ID is required.");

        RuleFor(x => x.ReceiverWalletNumber)
            .NotEmpty()
            .WithMessage("Receiver wallet number is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-character code.");
    }
}

