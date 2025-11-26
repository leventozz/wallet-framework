using FluentAssertions;
using NSubstitute;
using WF.Shared.Contracts.Result;
using WF.WalletService.Application.Abstractions;
using WF.WalletService.Application.Dtos;
using WF.WalletService.Application.Dtos.Filters;
using WF.WalletService.Application.Features.Admin.Queries.GetAdminWallets;
using Xunit;

namespace WF.WalletService.UnitTests.Application.Features.Admin.Queries.GetAdminWallets;

public class GetAdminWalletsQueryHandlerTests
{
    private readonly IAdminWalletQueryService _queryService;
    private readonly GetAdminWalletsQueryHandler _handler;
    private readonly Bogus.Faker _faker;

    public GetAdminWalletsQueryHandlerTests()
    {
        _queryService = Substitute.For<IAdminWalletQueryService>();
        _handler = new GetAdminWalletsQueryHandler(_queryService);
        _faker = new Bogus.Faker();
    }

    private GetAdminWalletsQuery CreateValidQuery()
    {
        return new GetAdminWalletsQuery
        {
            PageNumber = 1,
            PageSize = 20
        };
    }

    private PagedResult<AdminWalletListDto> CreatePagedResult()
    {
        return new PagedResult<AdminWalletListDto>(
            new List<AdminWalletListDto>(),
            0,
            1,
            20);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = CreateValidQuery();
        var expectedResult = CreatePagedResult();

        _queryService.GetWalletsAsync(
            Arg.Any<WalletListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEquivalentTo(expectedResult.Items);
        result.Value.TotalCount.Should().Be(expectedResult.TotalCount);
        result.Value.PageNumber.Should().Be(expectedResult.PageNumber);
        result.Value.PageSize.Should().Be(expectedResult.PageSize);
    }

    [Fact]
    public async Task Handle_ShouldPassFilterToQueryService()
    {
        // Arrange
        var query = new GetAdminWalletsQuery
        {
            PageNumber = 2,
            PageSize = 10,
            WalletNumber = "12345678",
            Currency = "TRY",
            IsActive = true,
            IsFrozen = false,
            IsClosed = false,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        var expectedResult = CreatePagedResult();

        _queryService.GetWalletsAsync(
            Arg.Any<WalletListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _queryService.Received(1).GetWalletsAsync(
            Arg.Is<WalletListFilter>(f =>
                f.PageNumber == query.PageNumber &&
                f.PageSize == query.PageSize &&
                f.WalletNumber == query.WalletNumber &&
                f.Currency == query.Currency &&
                f.IsActive == query.IsActive &&
                f.IsFrozen == query.IsFrozen &&
                f.IsClosed == query.IsClosed &&
                f.StartDate == query.StartDate &&
                f.EndDate == query.EndDate),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDefaultPagination_ShouldUseDefaults()
    {
        // Arrange
        var query = new GetAdminWalletsQuery();
        var expectedResult = CreatePagedResult();

        _queryService.GetWalletsAsync(
            Arg.Any<WalletListFilter>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _queryService.Received(1).GetWalletsAsync(
            Arg.Is<WalletListFilter>(f =>
                f.PageNumber == 1 &&
                f.PageSize == 20),
            Arg.Any<CancellationToken>());
    }
}

