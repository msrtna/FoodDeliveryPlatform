using Identity.Application.Abstractions.Authentication;
using Identity.Infrastructure.Identity;

namespace Identity.UnitTests.Identity.Authentication;

public class RefreshTokenServiceTests
{
    private readonly RefreshTokenService _sut = new();

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyToken()
    {
        // Act
        var token = _sut.GenerateToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var firstToken = _sut.GenerateToken();
        var secondToken = _sut.GenerateToken();

        // Assert
        Assert.NotEqual(firstToken, secondToken);
    }

    [Fact]
    public void HashToken_ShouldReturnSameHash_ForSameToken()
    {
        // Arrange
        const string token = "sample-refresh-token";

        // Act
        var firstHash = _sut.HashToken(token);
        var secondHash = _sut.HashToken(token);

        // Assert
        Assert.Equal(firstHash, secondHash);
    }

    [Fact]
    public void HashToken_ShouldReturnDifferentHashes_ForDifferentTokens()
    {
        // Act
        var firstHash = _sut.HashToken("token-one");
        var secondHash = _sut.HashToken("token-two");

        // Assert
        Assert.NotEqual(firstHash, secondHash);
    }

    [Fact]
    public void HashToken_ShouldReturn64CharacterHash()
    {
        // Act
        var hash = _sut.HashToken("sample-refresh-token");

        // Assert
        Assert.Equal(64, hash.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void HashToken_ShouldThrowArgumentException_WhenTokenIsEmpty(
        string? token)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _sut.HashToken(token!));
    }
}