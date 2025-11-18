using FluentValidation;

namespace WF.TransactionService.Application.Features.Transfers.Commands.CreateTransfer;

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !(!string.IsNullOrWhiteSpace(x.SenderCustomerNumber) && 
                         !string.IsNullOrWhiteSpace(x.ReceiverCustomerNumber) && 
                         x.SenderCustomerNumber == x.ReceiverCustomerNumber))
            .WithMessage("Sender and receiver cannot be the same customer.");

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

