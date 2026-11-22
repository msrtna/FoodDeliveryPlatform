using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Identity;
using Identity.Application.Features.Authentication.Login;
using Moq;

namespace Identity.UnitTests.Features.Authentication.Login;

public class LoginHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();

        _sut = new LoginHandler(
            _identityServiceMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsAreValid_ShouldReturnAccessToken()
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

        var expectedToken = new AccessTokenResult(
            "access-token",
            DateTime.UtcNow.AddMinutes(15));

        _identityServiceMock
            .Setup(service => service.AuthenticateAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns(expectedToken);

        // Act
        var result = await _sut.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken.Token, result.Token);
        Assert.Equal(expectedToken.ExpiresAtUtc, result.ExpiresAtUtc);

        _identityServiceMock.Verify(
            service => service.AuthenticateAsync(
                command.Email,
                command.Password,
                CancellationToken.None),
            Times.Once);

        _jwtTokenServiceMock.Verify(
            service => service.GenerateAccessToken(user),
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
    }
}