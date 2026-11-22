using Identity.Application.Abstractions.Identity;
using Identity.Application.Features.Account.UpdateProfile;
using Moq;

namespace Identity.UnitTests.Features.Account.UpdateProfile;

public sealed class UpdateProfileHandlerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly UpdateProfileHandler _handler;

    public UpdateProfileHandlerTests()
    {
        _handler = new UpdateProfileHandler(_identityServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_CallsIdentityService()
    {
        // Arrange
        var command = new UpdateProfileCommand(
            "user-123",
            "John",
            "Doe");

        _identityServiceMock
            .Setup(service => service.UpdateProfileAsync(
                command.UserId,
                command.FirstName,
                command.LastName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityOperationResult.Success(command.UserId));

        // Act
        var result = await _handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _identityServiceMock.Verify(service => service.UpdateProfileAsync(
            command.UserId,
            command.FirstName,
            command.LastName,
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenIdentityServiceFails_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateProfileCommand(
            "missing-user",
            "John",
            "Doe");

        var errors = new[]
        {
            new IdentityOperationError(
                "UserNotFound",
                "The user was not found.")
        };

        _identityServiceMock
            .Setup(service => service.UpdateProfileAsync(
                command.UserId,
                command.FirstName,
                command.LastName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityOperationResult.Failure(errors));

        // Act
        var result = await _handler.HandleAsync(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "UserNotFound");
    }
}