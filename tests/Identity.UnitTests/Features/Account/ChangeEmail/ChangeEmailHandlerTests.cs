using Identity.Application.Abstractions.Identity;
using Identity.Application.Features.Account.ChangeEmail;
using Moq;

namespace Identity.UnitTests.Features.Account.ChangeEmail;

public sealed class ChangeEmailHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly ChangeEmailHandler _handler;

    public ChangeEmailHandlerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();

        _handler = new ChangeEmailHandler(
            _identityServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailChangeSucceeds_ReturnsSuccess()
    {
        // Arrange
        var command = new ChangeEmailCommand(
            "user-123",
            "new@example.com",
            "CurrentPassword123!");

        var expectedResult = IdentityOperationResult.Success("user-123");

        _identityServiceMock
            .Setup(x => x.ChangeEmailAsync(
                command.UserId,
                command.NewEmail,
                command.CurrentPassword,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("user-123", result.UserId);

        _identityServiceMock.Verify(
            x => x.ChangeEmailAsync(
                command.UserId,
                command.NewEmail,
                command.CurrentPassword,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailChangeFails_ReturnsFailure()
    {
        // Arrange
        var command = new ChangeEmailCommand(
            "user-123",
            "existing@example.com",
            "CurrentPassword123!");

        var expectedResult = IdentityOperationResult.Failure(
            new[]
            {
                new IdentityOperationError(
                    "DuplicateEmail",
                    "This email address is already registered.")
            });

        _identityServiceMock
            .Setup(x => x.ChangeEmailAsync(
                command.UserId,
                command.NewEmail,
                command.CurrentPassword,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(
            result.Errors,
            error => error.Code == "DuplicateEmail");

        _identityServiceMock.Verify(
            x => x.ChangeEmailAsync(
                command.UserId,
                command.NewEmail,
                command.CurrentPassword,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}