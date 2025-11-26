using FluentAssertions;
using WF.Shared.Contracts.Result;
using WF.WalletService.Domain.ValueObjects;
using Xunit;

namespace WF.WalletService.UnitTests.Domain.ValueObjects;

public class IbanTests
{
    // Valid Turkish IBAN: TR330006100519786457841326
    // Valid format IBAN for testing: GB82WEST12345698765432 (valid check digits)
    private const string ValidIban = "GB82WEST12345698765432";
    private const string ValidTurkishIban = "TR330006100519786457841326";

    [Fact]
    public void Create_WithValidIban_ShouldReturnSuccess()
    {
        // Arrange
        var ibanValue = ValidIban;

        // Act
        var result = Iban.Create(ibanValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ibanValue);
    }

    [Fact]
    public void Create_WithValidTurkishIban_ShouldReturnSuccess()
    {
        // Arrange
        var ibanValue = ValidTurkishIban;

        // Act
        var result = Iban.Create(ibanValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ibanValue);
    }

    [Fact]
    public void Create_WithNullIban_ShouldReturnFailure()
    {
        // Arrange
        string? ibanValue = null;

        // Act
        var result = Iban.Create(ibanValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Iban.Required");
        result.Error.Message.Should().Be("IBAN cannot be null or empty.");
    }

    [Fact]
    public void Create_WithEmptyIban_ShouldReturnFailure()
    {
        // Arrange
        var ibanValue = string.Empty;

        // Act
        var result = Iban.Create(ibanValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Iban.Required");
        result.Error.Message.Should().Be("IBAN cannot be null or empty.");
    }

    [Fact]
    public void Create_WithWhitespaceIban_ShouldReturnFailure()
    {
        // Arrange
        var ibanValue = "   ";

        // Act
        var result = Iban.Create(ibanValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Iban.Required");
    }

    [Fact]
    public void Create_WithTooShortIban_ShouldReturnFailure()
    {
        // Arrange
        var ibanValue = "GB12"; // Too short

        // Act
        var result = Iban.Create(ibanValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Iban.InvalidLength");
        result.Error.Message.Should().Contain("15");
    }

    [Fact]
    public void Create_WithTooLongIban_ShouldReturnFailure()
    {
        // Arrange
        var ibanValue = "GB82WEST123456987654321234567890123"; // Too long (35 characters)

        // Act
        var result = Iban.Create(ibanValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Iban.InvalidLength");
        result.Error.Message.Should().Contain("34");
    }

    [Theory]
    [InlineData("12GBWEST12345698765432")] // Starts with digits
    [InlineData("G2WEST12345698765432")] // Only one letter in country code
    [InlineData("GBWEST12345698765432")] // Missing check digits
    [InlineData("GB8WEST12345698765432")] // Only one check digit
    [InlineData("GB82WEST1234569876543!")] // Invalid character
    public void Create_WithInvalidFormat_ShouldReturnFailure(string invalidIban)
    {
        // Act
        var result = Iban.Create(invalidIban);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Iban.InvalidFormat");
    }

    [Fact]
    public void Create_WithInvalidCheckDigits_ShouldReturnFailure()
    {
        // Arrange
        var ibanValue = "GB99WEST12345698765432"; // Invalid check digits

        // Act
        var result = Iban.Create(ibanValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Iban.InvalidCheckDigits");
        result.Error.Message.Should().Be("IBAN check digits are invalid.");
    }

    [Fact]
    public void Create_ShouldNormalizeIban_RemoveSpaces()
    {
        // Arrange
        var ibanWithSpaces = "GB 82 WE ST 12 34 56 98 76 54 32";

        // Act
        var result = Iban.Create(ibanWithSpaces);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().NotContain(" ");
        result.Value.Value.Should().Be("GB82WEST12345698765432");
    }

    [Fact]
    public void Create_ShouldNormalizeIban_ToUpperCase()
    {
        // Arrange
        var ibanLowercase = "gb82west12345698765432";

        // Act
        var result = Iban.Create(ibanLowercase);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("GB82WEST12345698765432");
    }

    [Fact]
    public void FromDatabaseValue_WithValidValue_ShouldReturnIban()
    {
        // Arrange
        var validIban = ValidIban;

        // Act
        var iban = Iban.FromDatabaseValue(validIban);

        // Assert
        iban.Value.Should().Be(validIban);
    }

    [Fact]
    public void FromDatabaseValue_WithNullValue_ShouldThrowException()
    {
        // Arrange
        string? nullValue = null;

        // Act & Assert
        var act = () => Iban.FromDatabaseValue(nullValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("IBAN cannot be null or empty when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithInvalidValue_ShouldThrowException()
    {
        // Arrange
        var invalidIban = "INVALID";

        // Act & Assert
        var act = () => Iban.FromDatabaseValue(invalidIban);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnValue()
    {
        // Arrange
        var ibanValue = ValidIban;
        var iban = Iban.Create(ibanValue).Value;

        // Act
        string result = iban;

        // Assert
        result.Should().Be(ibanValue);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var ibanValue = ValidIban;
        var iban = Iban.Create(ibanValue).Value;

        // Act
        var result = iban.ToString();

        // Assert
        result.Should().Be(ibanValue);
    }

    [Fact]
    public void ToFormattedString_ShouldReturnFormattedValue()
    {
        // Arrange
        var ibanValue = ValidIban;
        var iban = Iban.Create(ibanValue).Value;

        // Act
        var result = iban.ToFormattedString();

        // Assert
        result.Should().Contain(" ");
        result.Replace(" ", "").Should().Be(ibanValue);
    }
}

