using Identity.Domain.Entities;

namespace Identity.UnitTests.Domain.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        const string userId = "user-123";
        const string tokenHash = "sample-hash";
        var familyId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = new RefreshToken(
            userId,
            tokenHash,
            familyId,
            expiresAt);

        // Assert
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(tokenHash, token.TokenHash);
        Assert.Equal(familyId, token.FamilyId);
        Assert.Equal(expiresAt, token.ExpiresAtUtc);
        Assert.Null(token.RevokedAtUtc);
        Assert.Null(token.ReplacedByTokenId);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUserIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new RefreshToken(
                string.Empty,
                "sample-hash",
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTokenHashIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new RefreshToken(
                "user-123",
                string.Empty,
                Guid.NewGuid(),
                DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenExpirationIsInPast()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new RefreshToken(
                "user-123",
                "sample-hash",
                Guid.NewGuid(),
                DateTime.UtcNow.AddMinutes(-5)));
    }

    [Fact]
    public void Revoke_ShouldMarkTokenAsInactive()
    {
        // Arrange
        var token = CreateToken();

        // Act
        token.Revoke();

        // Assert
        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
        Assert.NotNull(token.RevokedAtUtc);
    }

    [Fact]
    public void Revoke_ShouldStoreReplacementTokenId()
    {
        // Arrange
        var token = CreateToken();
        var replacementId = Guid.NewGuid();

        // Act
        token.Revoke(replacementId);

        // Assert
        Assert.Equal(replacementId, token.ReplacedByTokenId);
    }

    private static RefreshToken CreateToken()
    {
        return new RefreshToken(
            "user-123",
            "sample-hash",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(7));
    }
}