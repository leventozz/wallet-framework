using FluentAssertions;
using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.CustomerService.UnitTests.Domain.ValueObjects;

public class PhoneNumberTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidPhoneNumber_ShouldReturnSuccess()
    {
        // Arrange
        var validPhoneNumber = _faker.Phone.PhoneNumber("+90##########");

        // Act
        var result = PhoneNumber.Create(validPhoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validPhoneNumber.Trim());
    }

    [Fact]
    public void Create_WithValidPhoneNumberWithFormatting_ShouldReturnSuccess()
    {
        // Arrange
        var validPhoneNumber = "+90 (555) 123-4567";

        // Act
        var result = PhoneNumber.Create(validPhoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validPhoneNumber);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldReturnFailure()
    {
        // Arrange
        var emptyValue = string.Empty;

        // Act
        var result = PhoneNumber.Create(emptyValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Required");
        result.Error.Message.Should().Be("Phone number cannot be null or empty.");
    }

    [Fact]
    public void Create_WithNullValue_ShouldReturnFailure()
    {
        // Arrange
        string? nullValue = null;

        // Act
        var result = PhoneNumber.Create(nullValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Required");
    }

    [Fact]
    public void Create_WithWhitespaceValue_ShouldReturnFailure()
    {
        // Arrange
        var whitespaceValue = "   ";

        // Act
        var result = PhoneNumber.Create(whitespaceValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Required");
    }

    [Fact]
    public void Create_WithTooShortValue_ShouldReturnFailure()
    {
        // Arrange
        var tooShortValue = "123456789"; // 9 characters, min is 10

        // Act
        var result = PhoneNumber.Create(tooShortValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.MinLength");
        result.Error.Message.Should().Contain("10");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldReturnFailure()
    {
        // Arrange
        var tooLongValue = new string('1', 21); // 21 characters, max is 20

        // Act
        var result = PhoneNumber.Create(tooLongValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.MaxLength");
        result.Error.Message.Should().Contain("20");
    }

    [Theory]
    [InlineData("1234567890a")] // contains letter
    [InlineData("1234567890@")] // contains @
    [InlineData("1234567890#")] // contains #
    [InlineData("1234567890!")] // contains !
    public void Create_WithInvalidCharacters_ShouldReturnFailure(string invalidPhoneNumber)
    {
        // Act
        var result = PhoneNumber.Create(invalidPhoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.InvalidFormat");
        result.Error.Message.Should().Contain("digits, spaces, hyphens, plus signs, and parentheses");
    }

    [Fact]
    public void Create_WithInsufficientDigits_ShouldReturnFailure()
    {
        // Arrange
        var insufficientDigits = "+90 (555) 12";

        // Act
        var result = PhoneNumber.Create(insufficientDigits);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.InsufficientDigits");
    }

    [Fact]
    public void Create_WithValidPhoneNumberContainingWhitespace_ShouldTrimAndReturnSuccess()
    {
        // Arrange
        var phoneNumber = "+905551234567";
        var phoneNumberWithWhitespace = "  " + phoneNumber + "  ";

        // Act
        var result = PhoneNumber.Create(phoneNumberWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(phoneNumber);
    }

    [Fact]
    public void FromDatabaseValue_WithValidValue_ShouldReturnPhoneNumber()
    {
        // Arrange
        var validPhoneNumber = "+905551234567";

        // Act
        var phoneNumber = PhoneNumber.FromDatabaseValue(validPhoneNumber);

        // Assert
        phoneNumber.Value.Should().Be(validPhoneNumber.Trim());
    }

    [Fact]
    public void FromDatabaseValue_WithNullValue_ShouldThrowException()
    {
        // Arrange
        string? nullValue = null;

        // Act & Assert
        var act = () => PhoneNumber.FromDatabaseValue(nullValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Phone number cannot be null or empty when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithEmptyValue_ShouldThrowException()
    {
        // Arrange
        var emptyValue = string.Empty;

        // Act & Assert
        var act = () => PhoneNumber.FromDatabaseValue(emptyValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Phone number cannot be null or empty when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithWhitespaceValue_ShouldTrimAndReturnPhoneNumber()
    {
        // Arrange
        var phoneNumber = "+905551234567";
        var whitespaceValue = "  " + phoneNumber + "  ";

        // Act
        var result = PhoneNumber.FromDatabaseValue(whitespaceValue);

        // Assert
        result.Value.Should().Be(phoneNumber);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnValue()
    {
        // Arrange
        var phoneNumberValue = "+905551234567";
        var phoneNumber = PhoneNumber.Create(phoneNumberValue).Value;

        // Act
        string result = phoneNumber;

        // Assert
        result.Should().Be(phoneNumberValue);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var phoneNumberValue = "+905551234567";
        var phoneNumber = PhoneNumber.Create(phoneNumberValue).Value;

        // Act
        var result = phoneNumber.ToString();

        // Assert
        result.Should().Be(phoneNumberValue);
    }
}

