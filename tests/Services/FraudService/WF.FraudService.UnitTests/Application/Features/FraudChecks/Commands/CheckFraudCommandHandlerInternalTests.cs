using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WF.FraudService.Application.Contracts;
using WF.FraudService.Application.Features.FraudChecks.Commands.CheckFraud;
using WF.Shared.Contracts.Abstractions;
using WF.Shared.Contracts.IntegrationEvents.Transaction;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Application.Features.FraudChecks.Commands;

public class CheckFraudCommandHandlerInternalTests
{
    private readonly IEnumerable<IFraudEvaluationRule> _rules;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CheckFraudCommandHandlerInternal> _logger;
    private readonly CheckFraudCommandHandlerInternal _handler;
    private readonly Bogus.Faker _faker;

    public CheckFraudCommandHandlerInternalTests()
    {
        _rules = new List<IFraudEvaluationRule>();
        _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<CheckFraudCommandHandlerInternal>>();
        _handler = new CheckFraudCommandHandlerInternal(_rules, _eventPublisher, _unitOfWork, _logger);
        _faker = new Bogus.Faker();
    }

    [Fact]
    public async Task Handle_WhenAllRulesPass_ShouldReturnTrueAndPublishApprovedEvent()
    {
        // Arrange
        var request = CreateValidRequest();
        var passingRule1 = Substitute.For<IFraudEvaluationRule>();
        passingRule1.Priority.Returns(1);
        passingRule1.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var passingRule2 = Substitute.For<IFraudEvaluationRule>();
        passingRule2.Priority.Returns(2);
        passingRule2.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var rules = new List<IFraudEvaluationRule> { passingRule1, passingRule2 };
        var handler = new CheckFraudCommandHandlerInternal(rules, _eventPublisher, _unitOfWork, _logger);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<FraudCheckApprovedEvent>(e => e.CorrelationId == request.CorrelationId),
            Arg.Any<CancellationToken>());

        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<FraudCheckDeclinedEvent>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAnyRuleFails_ShouldReturnFalseAndPublishDeclinedEvent()
    {
        // Arrange
        var request = CreateValidRequest();
        var passingRule = Substitute.For<IFraudEvaluationRule>();
        passingRule.Priority.Returns(1);
        passingRule.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var failingRule = Substitute.For<IFraudEvaluationRule>();
        failingRule.Priority.Returns(2);
        failingRule.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Failure("FraudCheck", "Rule failed")));

        var rules = new List<IFraudEvaluationRule> { passingRule, failingRule };
        var handler = new CheckFraudCommandHandlerInternal(rules, _eventPublisher, _unitOfWork, _logger);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<FraudCheckDeclinedEvent>(e => 
                e.CorrelationId == request.CorrelationId &&
                e.Reason == "Rule failed"),
            Arg.Any<CancellationToken>());

        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<FraudCheckApprovedEvent>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldExecuteRulesInPriorityOrder()
    {
        // Arrange
        var request = CreateValidRequest();
        var executionOrder = new List<int>();

        var rule1 = Substitute.For<IFraudEvaluationRule>();
        rule1.Priority.Returns(3);
        rule1.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(_ => executionOrder.Add(3));

        var rule2 = Substitute.For<IFraudEvaluationRule>();
        rule2.Priority.Returns(1);
        rule2.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(_ => executionOrder.Add(1));

        var rule3 = Substitute.For<IFraudEvaluationRule>();
        rule3.Priority.Returns(2);
        rule3.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(_ => executionOrder.Add(2));

        var rules = new List<IFraudEvaluationRule> { rule1, rule2, rule3 };
        var handler = new CheckFraudCommandHandlerInternal(rules, _eventPublisher, _unitOfWork, _logger);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        executionOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Handle_ShouldStopAtFirstFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var executionOrder = new List<int>();

        var rule1 = Substitute.For<IFraudEvaluationRule>();
        rule1.Priority.Returns(1);
        rule1.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(_ => executionOrder.Add(1));

        var rule2 = Substitute.For<IFraudEvaluationRule>();
        rule2.Priority.Returns(2);
        rule2.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.Failure("FraudCheck", "Rule 2 failed")))
            .AndDoes(_ => executionOrder.Add(2));

        var rule3 = Substitute.For<IFraudEvaluationRule>();
        rule3.Priority.Returns(3);
        rule3.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(_ => executionOrder.Add(3));

        var rules = new List<IFraudEvaluationRule> { rule1, rule2, rule3 };
        var handler = new CheckFraudCommandHandlerInternal(rules, _eventPublisher, _unitOfWork, _logger);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        executionOrder.Should().Equal(1, 2);
        await rule3.DidNotReceive().EvaluateAsync(Arg.Any<CheckFraudCommandInternal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChangesAfterPublishing()
    {
        // Arrange
        var request = CreateValidRequest();
        var passingRule = Substitute.For<IFraudEvaluationRule>();
        passingRule.Priority.Returns(1);
        passingRule.EvaluateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var rules = new List<IFraudEvaluationRule> { passingRule };
        var handler = new CheckFraudCommandHandlerInternal(rules, _eventPublisher, _unitOfWork, _logger);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<FraudCheckApprovedEvent>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private CheckFraudCommandInternal CreateValidRequest()
    {
        return new CheckFraudCommandInternal
        {
            CorrelationId = _faker.Random.Guid(),
            SenderCustomerId = _faker.Random.Guid(),
            ReceiverCustomerId = _faker.Random.Guid(),
            Amount = 100m,
            Currency = "USD",
            IpAddress = _faker.Internet.Ip()
        };
    }
}

