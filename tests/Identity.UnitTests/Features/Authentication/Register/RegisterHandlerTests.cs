using Identity.Application.Abstractions.Identity;
using Identity.Application.Features.Authentication.Register;
using Moq;

namespace Identity.UnitTests.Features.Authentication.Register;

public class RegisterHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly RegisterHandler _sut;

    public RegisterHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _sut = new RegisterHandler(_identityServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenRegistrationSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Password123!");

        var expectedResult = IdentityOperationResult.Success("user-123");

        _identityServiceMock
            .Setup(service => service.RegisterAsync(
                command.FirstName,
                command.LastName,
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("user-123", result.UserId);
        Assert.Empty(result.Errors);

        _identityServiceMock.Verify(
            service => service.RegisterAsync(
                command.FirstName,
                command.LastName,
                command.Email,
                command.Password,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRegistrationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Password123!");

        var expectedResult = IdentityOperationResult.Failure(
        [
            new IdentityOperationError(
                "DuplicateEmail",
                "Email is already registered.")
        ]);

        _identityServiceMock
            .Setup(service => service.RegisterAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);

        var error = Assert.Single(result.Errors);
        Assert.Equal("DuplicateEmail", error.Code);
    }
}