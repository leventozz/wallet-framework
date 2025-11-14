using FluentValidation;

namespace WF.TransactionService.Application.Features.Transfers.Commands.CreateTransfer;

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.SenderCustomerNumber) || !string.IsNullOrWhiteSpace(x.SenderWalletNumber))
            .WithMessage("Either sender customer number or sender wallet number must be provided.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.ReceiverCustomerNumber) || !string.IsNullOrWhiteSpace(x.ReceiverWalletNumber))
            .WithMessage("Either receiver customer number or receiver wallet number must be provided.");

        RuleFor(x => x)
            .Must(x => !(!string.IsNullOrWhiteSpace(x.SenderCustomerNumber) && 
                         !string.IsNullOrWhiteSpace(x.ReceiverCustomerNumber) && 
                         x.SenderCustomerNumber == x.ReceiverCustomerNumber))
            .WithMessage("Sender and receiver cannot be the same customer.");

        RuleFor(x => x)
            .Must(x => !(!string.IsNullOrWhiteSpace(x.SenderWalletNumber) && 
                         !string.IsNullOrWhiteSpace(x.ReceiverWalletNumber) && 
                         x.SenderWalletNumber == x.ReceiverWalletNumber))
            .WithMessage("Sender and receiver cannot be the same wallet.");

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

