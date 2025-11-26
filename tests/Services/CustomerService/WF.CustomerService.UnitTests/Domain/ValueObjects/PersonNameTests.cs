using FluentAssertions;
using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.CustomerService.UnitTests.Domain.ValueObjects;

public class PersonNameTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidNames_ShouldReturnSuccess()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName.Trim());
        result.Value.LastName.Should().Be(lastName.Trim());
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldReturnFailure()
    {
        // Arrange
        var firstName = string.Empty;
        var lastName = _faker.Name.LastName();

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.FirstName.Required");
        result.Error.Message.Should().Be("First name cannot be null or empty.");
    }

    [Fact]
    public void Create_WithNullFirstName_ShouldReturnFailure()
    {
        // Arrange
        string? firstName = null;
        var lastName = _faker.Name.LastName();

        // Act
        var result = PersonName.Create(firstName!, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.FirstName.Required");
        result.Error.Message.Should().Be("First name cannot be null or empty.");
    }

    [Fact]
    public void Create_WithWhitespaceFirstName_ShouldReturnFailure()
    {
        // Arrange
        var firstName = "   ";
        var lastName = _faker.Name.LastName();

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.FirstName.Required");
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldReturnFailure()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = string.Empty;

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.LastName.Required");
        result.Error.Message.Should().Be("Last name cannot be null or empty.");
    }

    [Fact]
    public void Create_WithNullLastName_ShouldReturnFailure()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        string? lastName = null;

        // Act
        var result = PersonName.Create(firstName, lastName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.LastName.Required");
        result.Error.Message.Should().Be("Last name cannot be null or empty.");
    }

    [Fact]
    public void Create_WithWhitespaceLastName_ShouldReturnFailure()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = "   ";

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.LastName.Required");
    }

    [Fact]
    public void Create_WithFirstNameExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var firstName = new string('A', 101); // 101 characters, max is 100
        var lastName = _faker.Name.LastName();

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.FirstName.MaxLength");
        result.Error.Message.Should().Contain("100");
    }

    [Fact]
    public void Create_WithLastNameExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = new string('A', 101); // 101 characters, max is 100

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.LastName.MaxLength");
        result.Error.Message.Should().Contain("100");
    }

    [Theory]
    [InlineData("John123")] // contains numbers
    [InlineData("John@Doe")] // contains @
    [InlineData("John#Doe")] // contains #
    [InlineData("John$Doe")] // contains $
    [InlineData("John_Doe")] // contains underscore
    [InlineData("John[Doe")] // contains [
    public void Create_WithInvalidFormatFirstName_ShouldReturnFailure(string invalidFirstName)
    {
        // Arrange
        var lastName = _faker.Name.LastName();

        // Act
        var result = PersonName.Create(invalidFirstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.FirstName.InvalidFormat");
        result.Error.Message.Should().Contain("letters, spaces, hyphens, apostrophes, and periods");
    }

    [Theory]
    [InlineData("Doe123")] // contains numbers
    [InlineData("Doe@Smith")] // contains @
    [InlineData("Doe#Smith")] // contains #
    [InlineData("Doe$Smith")] // contains $
    [InlineData("Doe_Smith")] // contains underscore
    [InlineData("Doe]Smith")] // contains ]
    public void Create_WithInvalidFormatLastName_ShouldReturnFailure(string invalidLastName)
    {
        // Arrange
        var firstName = _faker.Name.FirstName();

        // Act
        var result = PersonName.Create(firstName, invalidLastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.LastName.InvalidFormat");
        result.Error.Message.Should().Contain("letters, spaces, hyphens, apostrophes, and periods");
    }

    [Theory]
    [InlineData("Mary-Jane", "Watson")] // hyphen in first name
    [InlineData("O'Brien", "Smith")] // apostrophe in first name
    [InlineData("Dr.", "Smith")] // period in first name
    [InlineData("Jean-Luc", "Picard")] // hyphen in first name
    [InlineData("Mary", "O'Connor")] // apostrophe in last name
    [InlineData("John", "St. James")] // period and space in last name
    [InlineData("Mary Jane", "Watson")] // space in first name
    [InlineData("John", "Van Der Berg")] // multiple spaces in last name
    public void Create_WithValidSpecialCharacters_ShouldReturnSuccess(string firstName, string lastName)
    {
        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName.Trim());
        result.Value.LastName.Should().Be(lastName.Trim());
    }

    [Fact]
    public void Create_WithNamesContainingWhitespace_ShouldTrimAndReturnSuccess()
    {
        // Arrange
        var firstName = "  " + _faker.Name.FirstName() + "  ";
        var lastName = "  " + _faker.Name.LastName() + "  ";

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName.Trim());
        result.Value.LastName.Should().Be(lastName.Trim());
    }

    [Fact]
    public void FullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var personName = PersonName.Create(firstName, lastName).Value;

        // Act
        var fullName = personName.FullName;

        // Assert
        fullName.Should().Be($"{firstName.Trim()} {lastName.Trim()}");
    }

    [Fact]
    public void ToString_ShouldReturnFullName()
    {
        // Arrange
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var personName = PersonName.Create(firstName, lastName).Value;

        // Act
        var result = personName.ToString();

        // Assert
        result.Should().Be($"{firstName.Trim()} {lastName.Trim()}");
        result.Should().Be(personName.FullName);
    }

    [Fact]
    public void Create_WithMaxLengthNames_ShouldReturnSuccess()
    {
        // Arrange
        var firstName = new string('A', 100); // exactly 100 characters
        var lastName = new string('B', 100); // exactly 100 characters

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
    }

    [Fact]
    public void Create_WithSingleCharacterNames_ShouldReturnSuccess()
    {
        // Arrange
        var firstName = "A";
        var lastName = "B";

        // Act
        var result = PersonName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
    }
}

