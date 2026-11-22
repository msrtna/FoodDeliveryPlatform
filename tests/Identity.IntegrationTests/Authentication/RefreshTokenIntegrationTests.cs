
using System.Net;
using System.Net.Http.Json;
using Identity.IntegrationTests.Infrastructure;

namespace Identity.IntegrationTests.Authentication;

public sealed class RefreshTokenIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RefreshTokenIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        var tokens = await RegisterAndLoginAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/authentication/refresh",
            new { RefreshToken = tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var newTokens =
            await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(newTokens);
        Assert.False(string.IsNullOrWhiteSpace(newTokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(newTokens.RefreshToken));

        Assert.NotEqual(tokens.RefreshToken, newTokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_ReusingOldToken_ReturnsUnauthorized()
    {
        var tokens = await RegisterAndLoginAsync();

        var firstRefresh = await _client.PostAsJsonAsync(
            "/api/authentication/refresh",
            new { RefreshToken = tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);

        var secondRefresh = await _client.PostAsJsonAsync(
            "/api/authentication/refresh",
            new { RefreshToken = tokens.RefreshToken });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            secondRefresh.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/authentication/refresh",
            new { RefreshToken = "unknown-refresh-token" });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private async Task<TokenResponse> RegisterAndLoginAsync()
    {
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

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokens =
            await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(tokens);

        return tokens;
    }

    private sealed record TokenResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);
}
