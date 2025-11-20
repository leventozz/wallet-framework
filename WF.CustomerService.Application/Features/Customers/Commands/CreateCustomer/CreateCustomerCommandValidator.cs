using FluentValidation;

namespace WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        private const int MaxFirstNameLength = 100;
        private const int MaxLastNameLength = 100;
        private const int MaxEmailLength = 320;
        private const int MaxPhoneNumberLength = 20;
        private const int MinPhoneNumberLength = 10;

        private const int MinPasswordLength = 8;
        private const int MaxPasswordLength = 128;

        public CreateCustomerCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(MaxFirstNameLength)
                .WithMessage($"First name must not exceed {MaxFirstNameLength} characters.")
                .Matches(@"^[a-zA-Z\s\-'\.]+$")
                .WithMessage("First name can only contain letters, spaces, hyphens, apostrophes, and periods.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(MaxLastNameLength)
                .WithMessage($"Last name must not exceed {MaxLastNameLength} characters.")
                .Matches(@"^[a-zA-Z\s\-'\.]+$")
                .WithMessage("Last name can only contain letters, spaces, hyphens, apostrophes, and periods.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email must be a valid email address.")
                .MaximumLength(MaxEmailLength)
                .WithMessage($"Email must not exceed {MaxEmailLength} characters.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(MinPasswordLength)
                .WithMessage($"Password must be at least {MinPasswordLength} characters.")
                .MaximumLength(MaxPasswordLength)
                .WithMessage($"Password must not exceed {MaxPasswordLength} characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .MinimumLength(MinPhoneNumberLength)
                .WithMessage($"Phone number must be at least {MinPhoneNumberLength} characters.")
                .MaximumLength(MaxPhoneNumberLength)
                .WithMessage($"Phone number must not exceed {MaxPhoneNumberLength} characters.")
                .Matches(@"^[\d\s\-\+\(\)]+$")
                .WithMessage("Phone number can only contain digits, spaces, hyphens, plus signs, and parentheses.");
        }
    }
}
