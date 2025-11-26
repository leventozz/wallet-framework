using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.CustomerService.Application.Abstractions.Identity;
using WF.CustomerService.Application.Features.Customers.Commands.CreateCustomer;
using WF.CustomerService.Domain.Abstractions;
using WF.CustomerService.Domain.Entities;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Customer;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.CustomerService.UnitTests.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IIdentityService _identityService;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;
    private readonly CreateCustomerCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CreateCustomerCommandHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _identityService = Substitute.For<IIdentityService>();
        _integrationEventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<CreateCustomerCommandHandler>>();
        _handler = new CreateCustomerCommandHandler(
            _customerRepository,
            _identityService,
            _integrationEventPublisher,
            _unitOfWork,
            _logger);
        _faker = new Bogus.Faker();
    }

    private CreateCustomerCommand CreateValidCommand()
    {
        return new CreateCustomerCommand
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password(12),
            ConfirmPassword = string.Empty,
            PhoneNumber = _faker.Phone.PhoneNumber("+90##########")
        };
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            command.Email,
            command.Password,
            command.FirstName,
            command.LastName,
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _identityService.Received(1).RegisterUserAsync(
            command.Email,
            command.Password,
            command.FirstName,
            command.LastName,
            Arg.Any<CancellationToken>());

        await _customerRepository.Received().IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _customerRepository.Received(1).AddCustomerAsync(Arg.Any<Customer>());

        await _integrationEventPublisher.Received(1).PublishAsync(
            Arg.Is<CustomerCreatedEvent>(e => e.CustomerId == result.Value),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceFails_ShouldThrowException()
    {
        // Arrange
        var command = CreateValidCommand();
        var exception = new Exception("Identity service error");

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(exception));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Identity service error");

        await _customerRepository.DidNotReceive().AddCustomerAsync(Arg.Any<Customer>());
        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<CustomerCreatedEvent>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomerNumberGenerationFails_ShouldThrowException()
    {
        // Arrange
        var command = CreateValidCommand();
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        // All attempts return false (not unique)
        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unable to generate a unique customer number after 5 attempts*");

        await _customerRepository.DidNotReceive().AddCustomerAsync(Arg.Any<Customer>());
        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<CustomerCreatedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPersonNameCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = string.Empty, // Invalid first name
            LastName = _faker.Name.LastName(),
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password(12),
            ConfirmPassword = string.Empty,
            PhoneNumber = _faker.Phone.PhoneNumber("+90##########")
        };
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PersonName.FirstName.Required");

        await _customerRepository.DidNotReceive().AddCustomerAsync(Arg.Any<Customer>());
        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<CustomerCreatedEvent>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            Email = "invalid-email", // Invalid email
            Password = _faker.Internet.Password(12),
            ConfirmPassword = string.Empty,
            PhoneNumber = _faker.Phone.PhoneNumber("+90##########")
        };
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.InvalidFormat");

        await _customerRepository.DidNotReceive().AddCustomerAsync(Arg.Any<Customer>());
        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<CustomerCreatedEvent>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPhoneNumberCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password(12),
            ConfirmPassword = string.Empty,
            PhoneNumber = "123" // Invalid phone number (too short)
        };
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.MinLength");

        await _customerRepository.DidNotReceive().AddCustomerAsync(Arg.Any<Customer>());
        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<CustomerCreatedEvent>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _integrationEventPublisher.Received(1).PublishAsync(
            Arg.Is<CustomerCreatedEvent>(e => e.CustomerId == result.Value),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRetryCustomerNumberGeneration_WhenNotUnique()
    {
        // Arrange
        var command = CreateValidCommand();
        var identityId = _faker.Random.Guid().ToString();

        _identityService.RegisterUserAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(identityId);

        // First 2 attempts return false, third returns true
        _customerRepository.IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(false, false, true);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should have been called 3 times (2 failures + 1 success)
        await _customerRepository.Received(3).IsCustomerNumberUniqueAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _customerRepository.Received(1).AddCustomerAsync(Arg.Any<Customer>());
    }
}

