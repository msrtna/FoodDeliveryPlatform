using Identity.Application.Features.Authentication.Refresh;

namespace Identity.UnitTests.Features.Authentication.Refresh;

public sealed class RefreshCommandValidatorTests
{
    private readonly RefreshCommandValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WhenRefreshTokenIsValid_ReturnsSuccess()
    {
        // Arrange
        var command = new RefreshCommand("valid-refresh-token");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ValidateAsync_WhenRefreshTokenIsEmpty_ReturnsFailure(
        string refreshToken)
    {
        // Arrange
        var command = new RefreshCommand(refreshToken);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(command.RefreshToken));
    }
}