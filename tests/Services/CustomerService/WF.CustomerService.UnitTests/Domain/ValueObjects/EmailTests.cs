using FluentAssertions;
using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.CustomerService.UnitTests.Domain.ValueObjects;

public class EmailTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var validEmail = _faker.Internet.Email();

        // Act
        var result = Email.Create(validEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validEmail.Trim());
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldReturnFailure()
    {
        // Arrange
        var emptyEmail = string.Empty;

        // Act
        var result = Email.Create(emptyEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Required");
        result.Error.Message.Should().Be("Email cannot be null or empty.");
    }

    [Fact]
    public void Create_WithNullEmail_ShouldReturnFailure()
    {
        // Arrange
        string? nullEmail = null;

        // Act
        var result = Email.Create(nullEmail!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Required");
        result.Error.Message.Should().Be("Email cannot be null or empty.");
    }

    [Fact]
    public void Create_WithWhitespaceEmail_ShouldReturnFailure()
    {
        // Arrange
        var whitespaceEmail = "   ";

        // Act
        var result = Email.Create(whitespaceEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    [InlineData("user domain.com")]
    public void Create_WithInvalidFormat_ShouldReturnFailure(string invalidEmail)
    {
        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.InvalidFormat");
    }

    [Fact]
    public void Create_WithExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var longEmail = new string('a', 300) + "@" + new string('b', 20) + ".com"; // 320+ characters

        // Act
        var result = Email.Create(longEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.MaxLength");
        result.Error.Message.Should().Contain("320");
    }

    [Fact]
    public void Create_WithEmailContainingWhitespace_ShouldTrimAndReturnSuccess()
    {
        // Arrange
        var emailWithWhitespace = "  " + _faker.Internet.Email() + "  ";

        // Act
        var result = Email.Create(emailWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(emailWithWhitespace.Trim());
    }

    [Fact]
    public void FromDatabaseValue_WithValidValue_ShouldReturnEmail()
    {
        // Arrange
        var validEmail = _faker.Internet.Email();

        // Act
        var email = Email.FromDatabaseValue(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.Trim());
    }

    [Fact]
    public void FromDatabaseValue_WithNullValue_ShouldThrowException()
    {
        // Arrange
        string? nullValue = null;

        // Act & Assert
        var act = () => Email.FromDatabaseValue(nullValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Email cannot be null or empty when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithEmptyValue_ShouldThrowException()
    {
        // Arrange
        var emptyValue = string.Empty;

        // Act & Assert
        var act = () => Email.FromDatabaseValue(emptyValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Email cannot be null or empty when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithWhitespaceValue_ShouldTrimAndReturnEmail()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var whitespaceValue = "  " + email + "  ";

        // Act
        var result = Email.FromDatabaseValue(whitespaceValue);

        // Assert
        result.Value.Should().Be(email);
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnValue()
    {
        // Arrange
        var emailValue = _faker.Internet.Email();
        var email = Email.Create(emailValue).Value;

        // Act
        string result = email;

        // Assert
        result.Should().Be(emailValue.Trim());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var emailValue = _faker.Internet.Email();
        var email = Email.Create(emailValue).Value;

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be(emailValue.Trim());
    }
}


