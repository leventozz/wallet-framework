using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.CustomerService.Application.Features.Customers.Commands.CloseCustomer;
using WF.CustomerService.Domain.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.CustomerService.UnitTests.Application.Features.Customers.Commands.CloseCustomer;

public class CloseCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CloseCustomerCommandHandler> _logger;
    private readonly CloseCustomerCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CloseCustomerCommandHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<CloseCustomerCommandHandler>>();
        _handler = new CloseCustomerCommandHandler(
            _customerRepository,
            _unitOfWork,
            _logger);
        _faker = new Bogus.Faker();
    }

    private Customer CreateValidCustomer()
    {
        var identityId = _faker.Random.Guid().ToString();
        var customerNumber = _faker.Random.AlphaNumeric(8);
        var name = PersonName.Create(_faker.Name.FirstName(), _faker.Name.LastName()).Value;
        var email = Email.Create(_faker.Internet.Email()).Value;
        var phoneNumber = PhoneNumber.Create(_faker.Phone.PhoneNumber("+90##########")).Value;

        return Customer.Create(identityId, name, email, customerNumber, phoneNumber).Value;
    }

    private CloseCustomerCommand CreateValidCommand()
    {
        return new CloseCustomerCommand(_faker.Random.Guid());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCloseCustomerSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = CreateValidCustomer();

        _customerRepository.GetCustomerByIdAsync(command.CustomerId)
            .Returns(customer);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeFalse();

        await _customerRepository.Received(1).GetCustomerByIdAsync(command.CustomerId);
        await _customerRepository.Received(1).UpdateCustomerAsync(customer);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = CreateValidCommand();

        _customerRepository.GetCustomerByIdAsync(command.CustomerId)
            .Returns((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain("Customer");

        await _customerRepository.Received(1).GetCustomerByIdAsync(command.CustomerId);
        await _customerRepository.DidNotReceive().UpdateCustomerAsync(Arg.Any<Customer>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSetActiveFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = CreateValidCustomer();
        
        // Soft delete the customer to make SetActive fail
        customer.SoftDelete();

        _customerRepository.GetCustomerByIdAsync(command.CustomerId)
            .Returns(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.Deleted");
        result.Error.Message.Should().Contain("Cannot change active status of a deleted customer");

        await _customerRepository.Received(1).GetCustomerByIdAsync(command.CustomerId);
        await _customerRepository.DidNotReceive().UpdateCustomerAsync(Arg.Any<Customer>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryMethods()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = CreateValidCustomer();

        _customerRepository.GetCustomerByIdAsync(command.CustomerId)
            .Returns(customer);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _customerRepository.Received(1).GetCustomerByIdAsync(command.CustomerId);
        await _customerRepository.Received(1).UpdateCustomerAsync(
            Arg.Is<Customer>(c => !c.IsActive));
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = CreateValidCustomer();

        _customerRepository.GetCustomerByIdAsync(command.CustomerId)
            .Returns(customer);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetCustomerAsInactive()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = CreateValidCustomer();
        customer.IsActive.Should().BeTrue(); // Verify initial state

        _customerRepository.GetCustomerByIdAsync(command.CustomerId)
            .Returns(customer);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeFalse();
    }
}

