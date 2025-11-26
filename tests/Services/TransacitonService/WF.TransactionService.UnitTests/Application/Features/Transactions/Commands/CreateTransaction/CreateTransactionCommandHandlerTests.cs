using FluentAssertions;
using NSubstitute;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.Dtos;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.TransactionService.Application.Abstractions;
using WF.TransactionService.Application.Features.Transactions.Commands.CreateTransaction;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.TransactionService.UnitTests.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandlerTests
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerServiceApiClient _customerServiceApiClient;
    private readonly IWalletServiceApiClient _walletServiceApiClient;
    private readonly IMachineContextProvider _machineContextProvider;
    private readonly CreateTransactionCommandHandler _handler;
    private readonly Bogus.Faker _faker;

    public CreateTransactionCommandHandlerTests()
    {
        _integrationEventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _customerServiceApiClient = Substitute.For<ICustomerServiceApiClient>();
        _walletServiceApiClient = Substitute.For<IWalletServiceApiClient>();
        _machineContextProvider = Substitute.For<IMachineContextProvider>();
        _handler = new CreateTransactionCommandHandler(
            _integrationEventPublisher,
            _unitOfWork,
            _customerServiceApiClient,
            _walletServiceApiClient,
            _machineContextProvider);
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
    public async Task Handle_WithValidCommand_ShouldCreateTransactionSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        var senderCustomerId = _faker.Random.Guid();
        var receiverCustomerId = _faker.Random.Guid();
        var senderWalletId = _faker.Random.Guid();
        var receiverWalletId = _faker.Random.Guid();
        var machineId = _faker.Random.Int(0, 1023);

        var senderCustomerLookup = new CustomerLookupDto
        {
            CustomerId = senderCustomerId,
            CustomerNumber = command.SenderCustomerNumber
        };

        var receiverCustomerLookup = new CustomerLookupDto
        {
            CustomerId = receiverCustomerId,
            CustomerNumber = command.ReceiverCustomerNumber
        };

        var senderWalletLookup = new WalletLookupDto
        {
            CustomerId = senderCustomerId,
            WalletId = senderWalletId
        };

        var receiverWalletLookup = new WalletLookupDto
        {
            CustomerId = receiverCustomerId,
            WalletId = receiverWalletId
        };

        _machineContextProvider.GetMachineId().Returns(machineId);
        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns(senderCustomerLookup);
        _customerServiceApiClient.LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>())
            .Returns(new List<CustomerLookupDto> { receiverCustomerLookup });
        _walletServiceApiClient.LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>())
            .Returns(new List<WalletLookupDto> { senderWalletLookup, receiverWalletLookup });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Should().StartWith("TX-");

        await _customerServiceApiClient.Received(1).GetCustomerByIdentityAsync(
            command.SenderIdentityId,
            Arg.Any<CancellationToken>());

        await _customerServiceApiClient.Received(1).LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>());

        await _walletServiceApiClient.Received(1).LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>());

        await _integrationEventPublisher.Received(1).PublishAsync(
            Arg.Is<TransferRequestStartedEvent>(e =>
                e.SenderCustomerId == senderCustomerId &&
                e.ReceiverCustomerId == receiverCustomerId &&
                e.SenderWalletId == senderWalletId &&
                e.ReceiverWalletId == receiverWalletId &&
                e.Amount == command.Amount &&
                e.Currency == command.Currency &&
                e.ClientIpAddress == command.ClientIpAddress &&
                e.TransactionId == result.Value),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSenderCustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();

        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns((CustomerLookupDto?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain("Sender customer");

        await _customerServiceApiClient.Received(1).GetCustomerByIdentityAsync(
            command.SenderIdentityId,
            Arg.Any<CancellationToken>());

        await _walletServiceApiClient.DidNotReceive().LookupByCustomerIdsAsync(
            Arg.Any<List<Guid>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<TransferRequestStartedEvent>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReceiverCustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var senderCustomerId = _faker.Random.Guid();

        var senderCustomerLookup = new CustomerLookupDto
        {
            CustomerId = senderCustomerId,
            CustomerNumber = command.SenderCustomerNumber
        };

        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns(senderCustomerLookup);
        _customerServiceApiClient.LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>())
            .Returns(new List<CustomerLookupDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain("Receiver");

        await _customerServiceApiClient.Received(1).GetCustomerByIdentityAsync(
            command.SenderIdentityId,
            Arg.Any<CancellationToken>());

        await _customerServiceApiClient.Received(1).LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>());

        await _walletServiceApiClient.DidNotReceive().LookupByCustomerIdsAsync(
            Arg.Any<List<Guid>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<TransferRequestStartedEvent>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSenderWalletNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var senderCustomerId = _faker.Random.Guid();
        var receiverCustomerId = _faker.Random.Guid();
        var receiverWalletId = _faker.Random.Guid();

        var senderCustomerLookup = new CustomerLookupDto
        {
            CustomerId = senderCustomerId,
            CustomerNumber = command.SenderCustomerNumber
        };

        var receiverCustomerLookup = new CustomerLookupDto
        {
            CustomerId = receiverCustomerId,
            CustomerNumber = command.ReceiverCustomerNumber
        };

        var receiverWalletLookup = new WalletLookupDto
        {
            CustomerId = receiverCustomerId,
            WalletId = receiverWalletId
        };

        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns(senderCustomerLookup);
        _customerServiceApiClient.LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>())
            .Returns(new List<CustomerLookupDto> { receiverCustomerLookup });
        _walletServiceApiClient.LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>())
            .Returns(new List<WalletLookupDto> { receiverWalletLookup });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain("Wallet");
        result.Error.Message.Should().Contain("Sender");

        await _walletServiceApiClient.Received(1).LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>());

        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<TransferRequestStartedEvent>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReceiverWalletNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var senderCustomerId = _faker.Random.Guid();
        var receiverCustomerId = _faker.Random.Guid();
        var senderWalletId = _faker.Random.Guid();

        var senderCustomerLookup = new CustomerLookupDto
        {
            CustomerId = senderCustomerId,
            CustomerNumber = command.SenderCustomerNumber
        };

        var receiverCustomerLookup = new CustomerLookupDto
        {
            CustomerId = receiverCustomerId,
            CustomerNumber = command.ReceiverCustomerNumber
        };

        var senderWalletLookup = new WalletLookupDto
        {
            CustomerId = senderCustomerId,
            WalletId = senderWalletId
        };

        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns(senderCustomerLookup);
        _customerServiceApiClient.LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>())
            .Returns(new List<CustomerLookupDto> { receiverCustomerLookup });
        _walletServiceApiClient.LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>())
            .Returns(new List<WalletLookupDto> { senderWalletLookup });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain("Wallet");
        result.Error.Message.Should().Contain("Receiver");

        await _walletServiceApiClient.Received(1).LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>());

        await _integrationEventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<TransferRequestStartedEvent>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishTransferRequestStartedEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var senderCustomerId = _faker.Random.Guid();
        var receiverCustomerId = _faker.Random.Guid();
        var senderWalletId = _faker.Random.Guid();
        var receiverWalletId = _faker.Random.Guid();
        var machineId = _faker.Random.Int(0, 1023);

        var senderCustomerLookup = new CustomerLookupDto
        {
            CustomerId = senderCustomerId,
            CustomerNumber = command.SenderCustomerNumber
        };

        var receiverCustomerLookup = new CustomerLookupDto
        {
            CustomerId = receiverCustomerId,
            CustomerNumber = command.ReceiverCustomerNumber
        };

        var senderWalletLookup = new WalletLookupDto
        {
            CustomerId = senderCustomerId,
            WalletId = senderWalletId
        };

        var receiverWalletLookup = new WalletLookupDto
        {
            CustomerId = receiverCustomerId,
            WalletId = receiverWalletId
        };

        _machineContextProvider.GetMachineId().Returns(machineId);
        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns(senderCustomerLookup);
        _customerServiceApiClient.LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>())
            .Returns(new List<CustomerLookupDto> { receiverCustomerLookup });
        _walletServiceApiClient.LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>())
            .Returns(new List<WalletLookupDto> { senderWalletLookup, receiverWalletLookup });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _integrationEventPublisher.Received(1).PublishAsync(
            Arg.Is<TransferRequestStartedEvent>(e =>
                e.CorrelationId != Guid.Empty &&
                e.TransactionId == result.Value &&
                e.SenderCustomerId == senderCustomerId &&
                e.SenderCustomerNumber == command.SenderCustomerNumber &&
                e.ReceiverCustomerId == receiverCustomerId &&
                e.ReceiverCustomerNumber == command.ReceiverCustomerNumber &&
                e.SenderWalletId == senderWalletId &&
                e.ReceiverWalletId == receiverWalletId &&
                e.Amount == command.Amount &&
                e.Currency == command.Currency &&
                e.ClientIpAddress == command.ClientIpAddress),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var senderCustomerId = _faker.Random.Guid();
        var receiverCustomerId = _faker.Random.Guid();
        var senderWalletId = _faker.Random.Guid();
        var receiverWalletId = _faker.Random.Guid();
        var machineId = _faker.Random.Int(0, 1023);

        var senderCustomerLookup = new CustomerLookupDto
        {
            CustomerId = senderCustomerId,
            CustomerNumber = command.SenderCustomerNumber
        };

        var receiverCustomerLookup = new CustomerLookupDto
        {
            CustomerId = receiverCustomerId,
            CustomerNumber = command.ReceiverCustomerNumber
        };

        var senderWalletLookup = new WalletLookupDto
        {
            CustomerId = senderCustomerId,
            WalletId = senderWalletId
        };

        var receiverWalletLookup = new WalletLookupDto
        {
            CustomerId = receiverCustomerId,
            WalletId = receiverWalletId
        };

        _machineContextProvider.GetMachineId().Returns(machineId);
        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns(senderCustomerLookup);
        _customerServiceApiClient.LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>())
            .Returns(new List<CustomerLookupDto> { receiverCustomerLookup });
        _walletServiceApiClient.LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>())
            .Returns(new List<WalletLookupDto> { senderWalletLookup, receiverWalletLookup });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueTransactionId()
    {
        // Arrange
        var command = CreateValidCommand();
        var senderCustomerId = _faker.Random.Guid();
        var receiverCustomerId = _faker.Random.Guid();
        var senderWalletId = _faker.Random.Guid();
        var receiverWalletId = _faker.Random.Guid();
        var machineId = _faker.Random.Int(0, 1023);

        var senderCustomerLookup = new CustomerLookupDto
        {
            CustomerId = senderCustomerId,
            CustomerNumber = command.SenderCustomerNumber
        };

        var receiverCustomerLookup = new CustomerLookupDto
        {
            CustomerId = receiverCustomerId,
            CustomerNumber = command.ReceiverCustomerNumber
        };

        var senderWalletLookup = new WalletLookupDto
        {
            CustomerId = senderCustomerId,
            WalletId = senderWalletId
        };

        var receiverWalletLookup = new WalletLookupDto
        {
            CustomerId = receiverCustomerId,
            WalletId = receiverWalletId
        };

        _machineContextProvider.GetMachineId().Returns(machineId);
        _customerServiceApiClient.GetCustomerByIdentityAsync(command.SenderIdentityId, Arg.Any<CancellationToken>())
            .Returns(senderCustomerLookup);
        _customerServiceApiClient.LookupByCustomerNumbersAsync(
            Arg.Is<List<string>>(l => l.Contains(command.ReceiverCustomerNumber)),
            Arg.Any<CancellationToken>())
            .Returns(new List<CustomerLookupDto> { receiverCustomerLookup });
        _walletServiceApiClient.LookupByCustomerIdsAsync(
            Arg.Is<List<Guid>>(l => l.Contains(senderCustomerId) && l.Contains(receiverCustomerId)),
            command.Currency,
            Arg.Any<CancellationToken>())
            .Returns(new List<WalletLookupDto> { senderWalletLookup, receiverWalletLookup });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Should().StartWith("TX-");
        result.Value.Length.Should().BeGreaterThan(3);
    }
}

