using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Identity;
using Identity.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.UnitTests.Identity.Authentication;

public class JwtTokenServiceTests
{
    private const string Issuer = "FoodDeliveryPlatform.Identity";
    private const string Audience = "FoodDeliveryPlatform";
    private const string SecretKey =
        "CHANGE_THIS_TO_A_SECURE_SECRET_KEY_WITH_AT_LEAST_32_BYTES";

    private static JwtTokenService CreateSut()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = Issuer,
            Audience = Audience,
            SecretKey = SecretKey,
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });

        return new JwtTokenService(options);
    }

    private static AuthenticatedUser CreateUser()
    {
        return new AuthenticatedUser(
            Id: "user-123",
            FirstName: "Mohammad",
            LastName: "Moradi",
            Email: "mohammad@example.com",
            Roles: ["Customer", "RestaurantOwner"]);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnTokenAndFutureExpiration()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = sut.GenerateAccessToken(CreateUser());

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainIssuerAudienceAndUserClaims()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = sut.GenerateAccessToken(CreateUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        // Assert
        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Contains(Audience, jwt.Audiences);

        Assert.Equal("user-123", jwt.Claims.Single(c => c.Type == "sub").Value);
        Assert.Equal(
            "mohammad@example.com",
            jwt.Claims.Single(c => c.Type == "email").Value);

        Assert.Equal(
            "Mohammad",
            jwt.Claims.Single(c => c.Type == "given_name").Value);

        Assert.Equal(
            "Moradi",
            jwt.Claims.Single(c => c.Type == "family_name").Value);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainUserRoles()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = sut.GenerateAccessToken(CreateUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        // Assert
        var roleClaims = jwt.Claims
            .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();

        Assert.Contains("Customer", roleClaims);
        Assert.Contains("RestaurantOwner", roleClaims);
    }

    [Fact]
    public void GenerateAccessToken_ShouldUseConfiguredExpiration()
    {
        // Arrange
        var sut = CreateSut();
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var result = sut.GenerateAccessToken(CreateUser());

        // Assert
        var expectedExpiration = beforeGeneration.AddMinutes(15);

        Assert.InRange(
            result.ExpiresAtUtc,
            expectedExpiration.AddSeconds(-1),
            expectedExpiration.AddSeconds(1));
    }
}