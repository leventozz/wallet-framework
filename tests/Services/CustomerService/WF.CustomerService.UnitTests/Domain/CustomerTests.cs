using FluentAssertions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.CustomerService.UnitTests.Domain;

public class CustomerTests
{
    private readonly Bogus.Faker _faker = new();

    private PersonName CreateValidPersonName()
    {
        return PersonName.Create(_faker.Name.FirstName(), _faker.Name.LastName()).Value;
    }

    private Email CreateValidEmail()
    {
        return Email.Create(_faker.Internet.Email()).Value;
    }

    private PhoneNumber CreateValidPhoneNumber()
    {
        return PhoneNumber.Create(_faker.Phone.PhoneNumber("+90##########")).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var identityId = _faker.Random.Guid().ToString();
        var customerNumber = _faker.Random.AlphaNumeric(8);
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Customer.Create(identityId, name, email, customerNumber, phoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IdentityId.Should().Be(identityId);
        result.Value.CustomerNumber.Should().Be(customerNumber);
        result.Value.Name.Should().Be(name);
        result.Value.Email.Should().Be(email);
        result.Value.PhoneNumber.Should().Be(phoneNumber);
    }

    [Fact]
    public void Create_WithEmptyIdentityId_ShouldReturnFailure()
    {
        // Arrange
        var identityId = string.Empty;
        var customerNumber = _faker.Random.AlphaNumeric(8);
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Customer.Create(identityId, name, email, customerNumber, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.IdentityId.Required");
        result.Error.Message.Should().Be("Identity ID cannot be null or empty.");
    }

    [Fact]
    public void Create_WithNullIdentityId_ShouldReturnFailure()
    {
        // Arrange
        string? identityId = null;
        var customerNumber = _faker.Random.AlphaNumeric(8);
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Customer.Create(identityId!, name, email, customerNumber, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.IdentityId.Required");
    }

    [Fact]
    public void Create_WithWhitespaceIdentityId_ShouldReturnFailure()
    {
        // Arrange
        var identityId = "   ";
        var customerNumber = _faker.Random.AlphaNumeric(8);
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Customer.Create(identityId, name, email, customerNumber, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.IdentityId.Required");
    }

    [Fact]
    public void Create_WithEmptyCustomerNumber_ShouldReturnFailure()
    {
        // Arrange
        var identityId = _faker.Random.Guid().ToString();
        var customerNumber = string.Empty;
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Customer.Create(identityId, name, email, customerNumber, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.CustomerNumber.Required");
        result.Error.Message.Should().Be("Customer number cannot be null or empty.");
    }

    [Fact]
    public void Create_WithNullCustomerNumber_ShouldReturnFailure()
    {
        // Arrange
        var identityId = _faker.Random.Guid().ToString();
        string? customerNumber = null;
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Customer.Create(identityId, name, email, customerNumber!, phoneNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.CustomerNumber.Required");
    }

    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        // Arrange
        var identityId = _faker.Random.Guid().ToString();
        var customerNumber = _faker.Random.AlphaNumeric(8);
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        // Act
        var result = Customer.Create(identityId, name, email, customerNumber, phoneNumber);
        var customer = result.Value;

        // Assert
        customer.Id.Should().NotBeEmpty();
        customer.KycStatus.Should().Be(KycStatus.Unverified);
        customer.IsDeleted.Should().BeFalse();
        customer.IsActive.Should().BeTrue();
        customer.UpdatedAtUtc.Should().BeNull();
        customer.DeletedBy.Should().BeNull();
        customer.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var newName = PersonName.Create(_faker.Name.FirstName(), _faker.Name.LastName()).Value;
        var newEmail = Email.Create(_faker.Internet.Email()).Value;
        var newPhoneNumber = PhoneNumber.Create(_faker.Phone.PhoneNumber("+90##########")).Value;

        // Act
        var result = customer.Update(newName, newEmail, newPhoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.Name.Should().Be(newName);
        customer.Email.Should().Be(newEmail);
        customer.PhoneNumber.Should().Be(newPhoneNumber);
        customer.UpdatedAtUtc.Should().NotBeNull();
        customer.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_WithPartialData_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var originalName = customer.Name;
        var originalEmail = customer.Email;
        var newPhoneNumber = PhoneNumber.Create(_faker.Phone.PhoneNumber("+90##########")).Value;

        // Act
        var result = customer.Update(phoneNumber: newPhoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.Name.Should().Be(originalName);
        customer.Email.Should().Be(originalEmail);
        customer.PhoneNumber.Should().Be(newPhoneNumber);
    }

    [Fact]
    public void Update_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.SoftDelete();
        var newName = PersonName.Create(_faker.Name.FirstName(), _faker.Name.LastName()).Value;

        // Act
        var result = customer.Update(name: newName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.Deleted");
        result.Error.Message.Should().Be("Cannot update a deleted customer.");
        customer.Name.Should().NotBe(newName);
    }

    [Fact]
    public void UpdateKycStatus_ShouldUpdateStatus()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var newStatus = KycStatus.EmailVerified;

        // Act
        var result = customer.UpdateKycStatus(newStatus);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.KycStatus.Should().Be(newStatus);
        customer.UpdatedAtUtc.Should().NotBeNull();
        customer.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateKycStatus_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.SoftDelete();
        var newStatus = KycStatus.EmailVerified;

        // Act
        var result = customer.UpdateKycStatus(newStatus);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.Deleted");
        result.Error.Message.Should().Be("Cannot update KYC status of a deleted customer.");
        customer.KycStatus.Should().NotBe(newStatus);
    }

    [Fact]
    public void SetActive_ShouldToggleActiveStatus()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.IsActive.Should().BeTrue();

        // Act
        var result = customer.SetActive(false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeFalse();
        customer.UpdatedAtUtc.Should().NotBeNull();
        customer.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Act - Set back to active
        var result2 = customer.SetActive(true);

        // Assert
        result2.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SetActive_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.SoftDelete();

        // Act
        var result = customer.SetActive(true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.Deleted");
        result.Error.Message.Should().Be("Cannot change active status of a deleted customer.");
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var customer = CreateValidCustomer();
        var deletedBy = _faker.Random.Guid().ToString();

        // Act
        var result = customer.SoftDelete(deletedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsDeleted.Should().BeTrue();
        customer.IsActive.Should().BeFalse();
        customer.DeletedBy.Should().Be(deletedBy);
        customer.UpdatedAtUtc.Should().NotBeNull();
        customer.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SoftDelete_WithoutDeletedBy_ShouldMarkAsDeleted()
    {
        // Arrange
        var customer = CreateValidCustomer();

        // Act
        var result = customer.SoftDelete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsDeleted.Should().BeTrue();
        customer.IsActive.Should().BeFalse();
        customer.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldReturnSuccess()
    {
        // Arrange
        var customer = CreateValidCustomer();
        customer.SoftDelete();
        var firstUpdatedAt = customer.UpdatedAtUtc;

        // Act
        var result = customer.SoftDelete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsDeleted.Should().BeTrue();
        customer.UpdatedAtUtc.Should().Be(firstUpdatedAt);
    }

    private Customer CreateValidCustomer()
    {
        var identityId = _faker.Random.Guid().ToString();
        var customerNumber = _faker.Random.AlphaNumeric(8);
        var name = CreateValidPersonName();
        var email = CreateValidEmail();
        var phoneNumber = CreateValidPhoneNumber();

        return Customer.Create(identityId, name, email, customerNumber, phoneNumber).Value;
    }
}


