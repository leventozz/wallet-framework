using FluentAssertions;
using FluentValidation.TestHelper;
using WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;
using Xunit;

namespace WF.TransactionService.UnitTests.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator;
    private readonly Bogus.Faker _faker;

    public CreateTransactionCommandValidatorTests()
    {
        _validator = new CreateTransactionCommandValidator();
        _faker = new Bogus.Faker();
    }

    private CreateTransactionCommand CreateValidCommand()
    {
        return new CreateTransactionCommand
        {
            SenderIdentityId = _faker.Random.Guid().ToString(),
            SenderCustomerNumber = _faker.Random.AlphaNumeric(8),
            ReceiverCustomerNumber = _faker.Random.AlphaNumeric(8),
            Amount = _faker.Random.Decimal(1, 10000),
            Currency = "USD",
            ClientIpAddress = _faker.Internet.Ip()
        };
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenReceiverCustomerNumberEmpty_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { ReceiverCustomerNumber = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReceiverCustomerNumber)
            .WithErrorMessage("Receiver customer number is required.");
    }

    [Fact]
    public void Validate_WhenReceiverCustomerNumberNull_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { ReceiverCustomerNumber = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReceiverCustomerNumber)
            .WithErrorMessage("Receiver customer number is required.");
    }

    [Fact]
    public void Validate_WhenReceiverCustomerNumberWhitespace_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { ReceiverCustomerNumber = "   " };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReceiverCustomerNumber)
            .WithErrorMessage("Receiver customer number is required.");
    }

    [Fact]
    public void Validate_WhenAmountIsZero_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Amount = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenAmountIsNegative_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Amount = _faker.Random.Decimal(-1000, -1) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenCurrencyEmpty_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Currency = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required.");
    }

    [Fact]
    public void Validate_WhenCurrencyNull_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Currency = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required.");
    }

    [Fact]
    public void Validate_WhenCurrencyLengthNotThree_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Currency = "US" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be a 3-character code.");
    }

    [Fact]
    public void Validate_WhenCurrencyLengthGreaterThanThree_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Currency = "USDD" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be a 3-character code.");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("TRY")]
    [InlineData("GBP")]
    public void Validate_WithValidThreeCharacterCurrency_ShouldPass(string currency)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Currency = currency };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Validate_WithPositiveAmount_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Amount = _faker.Random.Decimal(0.01m, 1000000) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}

