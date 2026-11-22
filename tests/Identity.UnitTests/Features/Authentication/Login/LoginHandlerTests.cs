using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Identity;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Features.Authentication.Login;
using Identity.Application.Features.Authentication.Common;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace Identity.UnitTests.Features.Authentication.Login;

public class LoginHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;

    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "FoodDeliveryPlatform.Identity",
            Audience = "FoodDeliveryPlatform",
            SecretKey = "CHANGE_THIS_TO_A_SECURE_SECRET_KEY_WITH_AT_LEAST_32_BYTES",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });

        _sut = new LoginHandler(
            _identityServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            jwtOptions);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsAreValid_ShouldReturnTokensAndPersistRefreshToken()
    {
        // Arrange
        var command = new LoginCommand(
            "mohammad@example.com",
            "Password123!");

        var user = new AuthenticatedUser(
            "user-123",
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            ["Customer"]);

        var accessToken = new AccessTokenResult(
            "access-token",
            DateTime.UtcNow.AddMinutes(15));

        const string rawRefreshToken = "raw-refresh-token";
        const string refreshTokenHash = "hashed-refresh-token";

        RefreshToken? savedRefreshToken = null;

        _identityServiceMock
            .Setup(service => service.AuthenticateAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns(accessToken);

        _refreshTokenServiceMock
            .Setup(service => service.GenerateToken())
            .Returns(rawRefreshToken);

        _refreshTokenServiceMock
            .Setup(service => service.HashToken(rawRefreshToken))
            .Returns(refreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>(
                (token, _) => savedRefreshToken = token)
            .Returns(Task.CompletedTask);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal(rawRefreshToken, result.RefreshToken);
        Assert.Equal(accessToken.ExpiresAtUtc, result.AccessTokenExpiresAtUtc);

        Assert.NotNull(savedRefreshToken);
        Assert.Equal("user-123", savedRefreshToken.UserId);
        Assert.Equal(refreshTokenHash, savedRefreshToken.TokenHash);
        Assert.NotEqual(rawRefreshToken, savedRefreshToken.TokenHash);

        _refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<RefreshToken>(),
                CancellationToken.None),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsAreInvalid_ShouldReturnNull()
    {
        // Arrange
        var command = new LoginCommand(
            "mohammad@example.com",
            "WrongPassword!");

        _identityServiceMock
            .Setup(service => service.AuthenticateAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticatedUser?)null);

        // Act
        var result = await _sut.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _jwtTokenServiceMock.Verify(
            service => service.GenerateAccessToken(
                It.IsAny<AuthenticatedUser>()),
            Times.Never);

        _refreshTokenServiceMock.Verify(
            service => service.GenerateToken(),
            Times.Never);

        _refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}