
using Identity.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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

    [Fact]
    public async Task UpdateProfile_WithValidAccessToken_UpdatesCurrentUserProfile()
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

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/authentication/login",
            new { Email = email, Password = password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokenResponse =
            await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(tokenResponse);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        // Act
        var updateResponse = await _client.PutAsJsonAsync(
            "/api/account/profile",
            new
            {
                FirstName = "Jane",
                LastName = "Smith"
            });

        // Assert update response
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify the updated profile
        var profileResponse = await _client.GetAsync("/api/account/me");

        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile =
            await profileResponse.Content.ReadFromJsonAsync<ProfileResponse>();

        Assert.NotNull(profile);
        Assert.Equal("Jane", profile.FirstName);
        Assert.Equal("Smith", profile.LastName);
        Assert.Equal(email, profile.Email);
    }

    [Fact]
    public async Task UpdateProfile_WithoutAccessToken_ReturnsUnauthorized()
    {
        // Arrange
        var originalAuthorization =
            _client.DefaultRequestHeaders.Authorization;

        try
        {
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PutAsJsonAsync(
                "/api/account/profile",
                new
                {
                    FirstName = "Jane",
                    LastName = "Smith"
                });

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
        finally
        {
            _client.DefaultRequestHeaders.Authorization =
                originalAuthorization;
        }
    }

    [Fact]
    public async Task UpdateProfile_WithInvalidData_ReturnsBadRequest()
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

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/authentication/login",
            new { Email = email, Password = password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokenResponse =
            await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(tokenResponse);

        var originalAuthorization =
            _client.DefaultRequestHeaders.Authorization;

        try
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    tokenResponse.AccessToken);

            // Act
            var response = await _client.PutAsJsonAsync(
                "/api/account/profile",
                new
                {
                    FirstName = "",
                    LastName = "Smith"
                });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally
        {
            _client.DefaultRequestHeaders.Authorization =
                originalAuthorization;
        }
    }

    [Fact]
    public async Task ChangeEmail_WithValidTokenAndCurrentPassword_UpdatesEmail()
    {
        // Arrange
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Authentication/register",
            new
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.email@example.com",
                Password = "Password123!"
            });

        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Authentication/login",
            new
            {
                Email = "john.email@example.com",
                Password = "Password123!"
            });

        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginContent.GetProperty("accessToken").GetString();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var request = new
        {
            NewEmail = "john.newemail@example.com",
            CurrentPassword = "Password123!"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            "/api/account/email",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var meResponse = await _client.GetAsync("/api/account/me");

        meResponse.EnsureSuccessStatusCode();

        var meContent = await meResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "john.newemail@example.com",
            meContent.GetProperty("email").GetString());
    }

    [Fact]
    public async Task ChangeEmail_WithoutAccessToken_ReturnsUnauthorized()
    {
        // Arrange
        var originalAuthorization = _client.DefaultRequestHeaders.Authorization;

        try
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var request = new
            {
                NewEmail = "new.email@example.com",
                CurrentPassword = "Password123!"
            };

            // Act
            var response = await _client.PutAsJsonAsync(
                "/api/account/email",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
        finally
        {
            _client.DefaultRequestHeaders.Authorization = originalAuthorization;
        }
    }

    [Fact]
    public async Task ChangeEmail_WithInvalidCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Authentication/register",
            new
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.invalidpassword@example.com",
                Password = "Password123!"
            });

        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Authentication/login",
            new
            {
                Email = "john.invalidpassword@example.com",
                Password = "Password123!"
            });

        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginContent.GetProperty("accessToken").GetString();

        var originalAuthorization = _client.DefaultRequestHeaders.Authorization;

        try
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            var request = new
            {
                NewEmail = "john.newemail@example.com",
                CurrentPassword = "WrongPassword123!"
            };

            // Act
            var response = await _client.PutAsJsonAsync(
                "/api/account/email",
                request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        finally
        {
            _client.DefaultRequestHeaders.Authorization = originalAuthorization;
        }
    }

    [Fact]
    public async Task ChangeEmail_WithExistingEmail_ReturnsConflict()
    {
        // Arrange
        var firstUserRegisterResponse = await _client.PostAsJsonAsync(
            "/api/Authentication/register",
            new
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.duplicate@example.com",
                Password = "Password123!"
            });

        firstUserRegisterResponse.EnsureSuccessStatusCode();

        var secondUserRegisterResponse = await _client.PostAsJsonAsync(
            "/api/Authentication/register",
            new
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.duplicate@example.com",
                Password = "Password123!"
            });

        secondUserRegisterResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Authentication/login",
            new
            {
                Email = "jane.duplicate@example.com",
                Password = "Password123!"
            });

        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginContent.GetProperty("accessToken").GetString();

        var originalAuthorization = _client.DefaultRequestHeaders.Authorization;

        try
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            var request = new
            {
                NewEmail = "john.duplicate@example.com",
                CurrentPassword = "Password123!"
            };

            // Act
            var response = await _client.PutAsJsonAsync(
                "/api/account/email",
                request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        finally
        {
            _client.DefaultRequestHeaders.Authorization = originalAuthorization;
        }
    }

    private sealed record ProfileResponse(
        string? UserId,
        string? Email,
        string? FirstName,
        string? LastName,
        string[] Roles);
    private sealed record LoginResponse(string AccessToken);
}
