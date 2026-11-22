
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Identity.IntegrationTests.Infrastructure;

namespace Identity.IntegrationTests.Authentication;

public sealed class AccountAuthenticationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountAuthenticationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMe_WithoutAccessToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/account/me");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidAccessToken_ReturnsOk()
    {
        // Arrange
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "StrongPassword123!";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/authentication/register",
            new
            {
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                Password = password
            });

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/authentication/login",
            new
            {
                Email = email,
                Password = password
            });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var tokenResponse = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(tokenResponse);
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse.AccessToken));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                tokenResponse.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/account/me");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    private sealed record LoginResponse(string AccessToken);
}
