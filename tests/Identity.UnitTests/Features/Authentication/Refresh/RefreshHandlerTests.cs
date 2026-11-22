using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Identity;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Features.Authentication.Common;
using Identity.Application.Features.Authentication.Refresh;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace Identity.UnitTests.Features.Authentication.Refresh;

public sealed class RefreshHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();

    private readonly JwtOptions _jwtOptions = new()
    {
        RefreshTokenExpirationDays = 7
    };

    private RefreshHandler CreateHandler()
    {
        return new RefreshHandler(
            _identityServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            Options.Create(_jwtOptions));
    }

    [Fact]
    public async Task HandleAsync_WhenTokenDoesNotExist_ReturnsNull()
    {
        // Arrange
        const string rawToken = "unknown-refresh-token";
        const string tokenHash = "UNKNOWN_HASH";

        _refreshTokenServiceMock
            .Setup(x => x.HashToken(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(
            new RefreshCommand(rawToken),
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _refreshTokenRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsExpired_ReturnsNull()
    {
        // Arrange
        const string rawToken = "expired-refresh-token";
        const string tokenHash = "EXPIRED_HASH";

        var refreshToken = new RefreshToken(
            "user-123",
            tokenHash,
            Guid.CreateVersion7(),
            DateTime.UtcNow.AddDays(7));

        typeof(RefreshToken)
            .GetProperty(nameof(RefreshToken.ExpiresAtUtc))!
            .SetValue(refreshToken, DateTime.UtcNow.AddDays(-1));

        _refreshTokenServiceMock
            .Setup(x => x.HashToken(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(
            new RefreshCommand(rawToken),
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _identityServiceMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsRevoked_RevokesTokenFamilyAndReturnsNull()
    {
        // Arrange
        const string rawToken = "revoked-refresh-token";
        const string tokenHash = "REVOKED_HASH";

        var familyId = Guid.CreateVersion7();

        var refreshToken = new RefreshToken(
            "user-123",
            tokenHash,
            familyId,
            DateTime.UtcNow.AddDays(7));

        refreshToken.Revoke();

        _refreshTokenServiceMock
            .Setup(x => x.HashToken(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(
            new RefreshCommand(rawToken),
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _refreshTokenRepositoryMock.Verify(
            x => x.RevokeFamilyAsync(
                familyId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_RevokesTokenFamilyAndReturnsNull()
    {
        // Arrange
        const string rawToken = "valid-refresh-token";
        const string tokenHash = "VALID_HASH";

        var familyId = Guid.CreateVersion7();

        var refreshToken = new RefreshToken(
            "user-123",
            tokenHash,
            familyId,
            DateTime.UtcNow.AddDays(7));

        _refreshTokenServiceMock
            .Setup(x => x.HashToken(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                tokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _identityServiceMock
            .Setup(x => x.GetByIdAsync(
                "user-123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticatedUser?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(
            new RefreshCommand(rawToken),
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _refreshTokenRepositoryMock.Verify(
            x => x.RevokeFamilyAsync(
                familyId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsValid_RotatesRefreshTokenAndReturnsNewTokens()
    {
        // Arrange
        const string rawToken = "old-refresh-token";
        const string oldTokenHash = "OLD_HASH";
        const string newRawToken = "new-refresh-token";
        const string newTokenHash = "NEW_HASH";

        var familyId = Guid.CreateVersion7();

        var existingToken = new RefreshToken(
            "user-123",
            oldTokenHash,
            familyId,
            DateTime.UtcNow.AddDays(7));

        var user = new AuthenticatedUser(
            "user-123",
            "John",
            "Doe",
            "john@example.com",
            new[] { "Customer" });

        var accessToken = new AccessTokenResult(
            "new-access-token",
            DateTime.UtcNow.AddMinutes(15));

        _refreshTokenServiceMock
            .Setup(x => x.HashToken(rawToken))
            .Returns(oldTokenHash);

        _refreshTokenServiceMock
            .Setup(x => x.GenerateToken())
            .Returns(newRawToken);

        _refreshTokenServiceMock
            .Setup(x => x.HashToken(newRawToken))
            .Returns(newTokenHash);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenHashAsync(
                oldTokenHash,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _identityServiceMock
            .Setup(x => x.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns(accessToken);

        var handler = CreateHandler();

        // Act
        var result = await handler.HandleAsync(
            new RefreshCommand(rawToken),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal(newRawToken, result.RefreshToken);
        Assert.Equal(accessToken.ExpiresAtUtc, result.AccessTokenExpiresAtUtc);

        Assert.True(existingToken.IsRevoked);
        Assert.NotNull(existingToken.ReplacedByTokenId);

        _refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(
                existingToken,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<RefreshToken>(token =>
                    token.UserId == user.Id &&
                    token.FamilyId == familyId &&
                    token.TokenHash == newTokenHash),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}