using System.Net;
using FluentAssertions;
using WF.FraudService.Domain.ValueObjects;
using WF.Shared.Contracts.Result;
using Xunit;

namespace WF.FraudService.UnitTests.Domain.ValueObjects;

public class IpAddressTests
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Create_WithValidIPv4_ShouldReturnSuccess()
    {
        // Arrange
        var validIp = _faker.Internet.Ip();

        // Act
        var result = IpAddress.Create(validIp);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.ToString().Should().Be(validIp);
    }

    [Fact]
    public void Create_WithValidIPv6_ShouldReturnSuccess()
    {
        // Arrange
        var validIpv6 = "2001:0db8:85a3:0000:0000:8a2e:0370:7334";
        var expectedNormalized = IPAddress.Parse(validIpv6).ToString();

        // Act
        var result = IpAddress.Create(validIpv6);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // IPv6 addresses are normalized by IPAddress.ToString() according to RFC 5952
        // Both formats represent the same IP address
        result.Value.Value.ToString().Should().Be(expectedNormalized);
        // Verify they represent the same IP address
        result.Value.Value.Should().Be(IPAddress.Parse(validIpv6));
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldReturnFailure()
    {
        // Arrange
        var emptyValue = string.Empty;

        // Act
        var result = IpAddress.Create(emptyValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IpAddress.Required");
        result.Error.Message.Should().Be("IP address cannot be null or empty.");
    }

    [Fact]
    public void Create_WithNullValue_ShouldReturnFailure()
    {
        // Arrange
        string? nullValue = null;

        // Act
        var result = IpAddress.Create(nullValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IpAddress.Required");
    }

    [Fact]
    public void Create_WithWhitespaceValue_ShouldReturnFailure()
    {
        // Arrange
        var whitespaceValue = "   ";

        // Act
        var result = IpAddress.Create(whitespaceValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IpAddress.Required");
    }

    [Theory]
    [InlineData("invalid-ip")]
    [InlineData("256.256.256.256")]
    [InlineData("192.168.1")]
    [InlineData("192.168.1.1.1")]
    public void Create_WithInvalidFormat_ShouldReturnFailure(string invalidIp)
    {
        // Act
        var result = IpAddress.Create(invalidIp);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IpAddress.InvalidFormat");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldTrimAndReturnSuccess()
    {
        // Arrange
        var validIp = _faker.Internet.Ip();
        var ipWithWhitespace = "  " + validIp + "  ";

        // Act
        var result = IpAddress.Create(ipWithWhitespace);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.ToString().Should().Be(validIp);
    }

    [Fact]
    public void FromDatabaseValue_WithValidValue_ShouldReturn()
    {
        // Arrange
        var validIp = _faker.Internet.Ip();

        // Act
        var ipAddress = IpAddress.FromDatabaseValue(validIp);

        // Assert
        ipAddress.Value.ToString().Should().Be(validIp);
    }

    [Fact]
    public void FromDatabaseValue_WithNullValue_ShouldThrow()
    {
        // Arrange
        string? nullValue = null;

        // Act & Assert
        var act = () => IpAddress.FromDatabaseValue(nullValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("IP address cannot be null or empty when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithEmptyValue_ShouldThrow()
    {
        // Arrange
        var emptyValue = string.Empty;

        // Act & Assert
        var act = () => IpAddress.FromDatabaseValue(emptyValue);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("IP address cannot be null or empty when reading from database.");
    }

    [Fact]
    public void FromDatabaseValue_WithInvalidFormat_ShouldThrow()
    {
        // Arrange
        var invalidIp = "invalid-ip";

        // Act & Assert
        var act = () => IpAddress.FromDatabaseValue(invalidIp);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid IP address format when reading from database.");
    }

    [Fact]
    public void ImplicitOperator_String_ShouldReturnValue()
    {
        // Arrange
        var ipValue = _faker.Internet.Ip();
        var ipAddress = IpAddress.Create(ipValue).Value;

        // Act
        string result = ipAddress;

        // Assert
        result.Should().Be(ipValue);
    }

    [Fact]
    public void ImplicitOperator_IPAddress_ShouldReturnValue()
    {
        // Arrange
        var ipValue = _faker.Internet.Ip();
        var ipAddress = IpAddress.Create(ipValue).Value;
        var expectedIp = IPAddress.Parse(ipValue);

        // Act
        IPAddress result = ipAddress;

        // Assert
        result.Should().Be(expectedIp);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var ipValue = _faker.Internet.Ip();
        var ipAddress = IpAddress.Create(ipValue).Value;

        // Act
        var result = ipAddress.ToString();

        // Assert
        result.Should().Be(ipValue);
    }
}

