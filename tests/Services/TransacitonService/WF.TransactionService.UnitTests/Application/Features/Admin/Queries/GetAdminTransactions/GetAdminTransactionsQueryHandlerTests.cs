using FluentAssertions;
using NSubstitute;
using WF.Shared.Contracts.Result;
using WF.TransactionService.Application.Contracts;
using WF.TransactionService.Application.Dtos;
using WF.TransactionService.Application.Dtos.Filters;
using WF.TransactionService.Application.Features.Admin.Queries.GetAdminTransactions;
using Xunit;

namespace WF.TransactionService.UnitTests.Application.Features.Admin.Queries.GetAdminTransactions;

public class GetAdminTransactionsQueryHandlerTests
{
    private readonly IAdminTransactionQueryService _queryService;
    private readonly GetAdminTransactionsQueryHandler _handler;
    private readonly Bogus.Faker _faker;

    public GetAdminTransactionsQueryHandlerTests()
    {
        _queryService = Substitute.For<IAdminTransactionQueryService>();
        _handler = new GetAdminTransactionsQueryHandler(_queryService);
        _faker = new Bogus.Faker();
    }

    private GetAdminTransactionsQuery CreateValidQuery()
    {
        return new GetAdminTransactionsQuery
        {
            PageNumber = 1,
            PageSize = 20,
            CorrelationId = _faker.Random.Guid(),
            TransactionId = $"TX-{_faker.Random.AlphaNumeric(10)}",
            CurrentState = "Pending",
            SenderCustomerNumber = _faker.Random.AlphaNumeric(8),
            ReceiverCustomerNumber = _faker.Random.AlphaNumeric(8),
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = CreateValidQuery();
        var expectedResult = PagedResult<AdminTransactionListDto>.Create(
            new List<AdminTransactionListDto>
            {
                new AdminTransactionListDto
                {
                    CorrelationId = query.CorrelationId!.Value,
                    TransactionId = query.TransactionId!,
                    CurrentState = query.CurrentState!,
                    SenderCustomerNumber = query.SenderCustomerNumber!,
                    ReceiverCustomerNumber = query.ReceiverCustomerNumber!,
                    Amount = _faker.Random.Decimal(1, 10000),
                    Currency = "USD",
                    CreatedAtUtc = DateTime.UtcNow,
                    CompletedAtUtc = null,
                    FailureReason = null,
                    ClientIpAddress = _faker.Internet.Ip()
                }
            },
            1,
            query.PageNumber,
            query.PageSize);

        _queryService.GetTransactionsAsync(
            Arg.Any<TransactionListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.PageNumber.Should().Be(query.PageNumber);
        result.Value.PageSize.Should().Be(query.PageSize);

        await _queryService.Received(1).GetTransactionsAsync(
            Arg.Any<TransactionListFilter>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapAllFilterPropertiesCorrectly()
    {
        // Arrange
        var query = CreateValidQuery();
        var expectedResult = PagedResult<AdminTransactionListDto>.Create(
            new List<AdminTransactionListDto>(),
            0,
            query.PageNumber,
            query.PageSize);

        _queryService.GetTransactionsAsync(
            Arg.Any<TransactionListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _queryService.Received(1).GetTransactionsAsync(
            Arg.Is<TransactionListFilter>(f =>
                f.PageNumber == query.PageNumber &&
                f.PageSize == query.PageSize &&
                f.CorrelationId == query.CorrelationId &&
                f.TransactionId == query.TransactionId &&
                f.CurrentState == query.CurrentState &&
                f.SenderCustomerNumber == query.SenderCustomerNumber &&
                f.ReceiverCustomerNumber == query.ReceiverCustomerNumber &&
                f.StartDate == query.StartDate &&
                f.EndDate == query.EndDate),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultPaginationValues()
    {
        // Arrange
        var query = new GetAdminTransactionsQuery
        {
            PageNumber = 1,
            PageSize = 20
        };

        var expectedResult = PagedResult<AdminTransactionListDto>.Create(
            new List<AdminTransactionListDto>(),
            0,
            query.PageNumber,
            query.PageSize);

        _queryService.GetTransactionsAsync(
            Arg.Any<TransactionListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _queryService.Received(1).GetTransactionsAsync(
            Arg.Is<TransactionListFilter>(f =>
                f.PageNumber == 1 &&
                f.PageSize == 20 &&
                f.CorrelationId == null &&
                f.TransactionId == null &&
                f.CurrentState == null &&
                f.SenderCustomerNumber == null &&
                f.ReceiverCustomerNumber == null &&
                f.StartDate == null &&
                f.EndDate == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var query = CreateValidQuery();
        var expectedResult = PagedResult<AdminTransactionListDto>.Create(
            new List<AdminTransactionListDto>(),
            0,
            query.PageNumber,
            query.PageSize);

        _queryService.GetTransactionsAsync(
            Arg.Any<TransactionListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldReturnAllItems()
    {
        // Arrange
        var query = CreateValidQuery();
        var items = new List<AdminTransactionListDto>
        {
            new AdminTransactionListDto
            {
                CorrelationId = _faker.Random.Guid(),
                TransactionId = $"TX-{_faker.Random.AlphaNumeric(10)}",
                CurrentState = "Pending",
                SenderCustomerNumber = _faker.Random.AlphaNumeric(8),
                ReceiverCustomerNumber = _faker.Random.AlphaNumeric(8),
                Amount = _faker.Random.Decimal(1, 10000),
                Currency = "USD",
                CreatedAtUtc = DateTime.UtcNow,
                CompletedAtUtc = null,
                FailureReason = null,
                ClientIpAddress = _faker.Internet.Ip()
            },
            new AdminTransactionListDto
            {
                CorrelationId = _faker.Random.Guid(),
                TransactionId = $"TX-{_faker.Random.AlphaNumeric(10)}",
                CurrentState = "Completed",
                SenderCustomerNumber = _faker.Random.AlphaNumeric(8),
                ReceiverCustomerNumber = _faker.Random.AlphaNumeric(8),
                Amount = _faker.Random.Decimal(1, 10000),
                Currency = "EUR",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                CompletedAtUtc = DateTime.UtcNow,
                FailureReason = null,
                ClientIpAddress = _faker.Internet.Ip()
            }
        };

        var expectedResult = PagedResult<AdminTransactionListDto>.Create(
            items,
            items.Count,
            query.PageNumber,
            query.PageSize);

        _queryService.GetTransactionsAsync(
            Arg.Any<TransactionListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }
}

