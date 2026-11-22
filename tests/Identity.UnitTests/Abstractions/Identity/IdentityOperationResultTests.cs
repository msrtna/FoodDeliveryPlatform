using Identity.Application.Abstractions.Identity;

namespace Identity.UnitTests.Abstractions.Identity;

public class IdentityOperationResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulResult_WithUserId()
    {
        // Arrange
        const string userId = "user-123";

        // Act
        var result = IdentityOperationResult.Success(userId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_ShouldReturnFailedResult_WithErrors()
    {
        // Arrange
        var errors = new List<IdentityOperationError>
        {
            new("DuplicateEmail", "Email is already registered.")
        };

        // Act
        var result = IdentityOperationResult.Failure(errors);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.UserId);
        Assert.Single(result.Errors);
        Assert.Equal("DuplicateEmail", result.Errors.First().Code);
    }
}