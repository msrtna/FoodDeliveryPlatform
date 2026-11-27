using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Restaurant.IntegrationTests.Infrastructure;
using Restaurant.Infrastructure.Persistence.Context;

namespace Restaurant.IntegrationTests.Restaurants;

public sealed class CreateRestaurantTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateRestaurantTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAndPersistsRestaurant()
    {
        // Arrange
        var request = new
        {
            Name = "Integration Test Restaurant",
            Description = "Restaurant created during integration testing.",
            Province = "Tehran",
            City = "Tehran",
            StreetAddress = "Valiasr Street",
            PostalCode = "1234567890"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/restaurants",
            request);

        // Assert - HTTP response
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Status: {(int)response.StatusCode}\nResponse: {responseBody}");

        Assert.NotNull(response.Headers.Location);

        var responseContent = await response.Content.ReadAsStringAsync();

        using var jsonDocument = JsonDocument.Parse(responseContent);

        var restaurantId = jsonDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        Assert.NotEqual(Guid.Empty, restaurantId);

        // Assert - Database persistence
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var restaurant = await dbContext.Restaurants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == restaurantId);

        Assert.NotNull(restaurant);
        Assert.Equal(request.Name, restaurant.Name);
        Assert.Equal(request.Province, restaurant.Address.Province);
        Assert.Equal(request.City, restaurant.Address.City);
    }


    [Fact]
    public async Task GetById_WhenRestaurantExists_ReturnsRestaurant()
    {
        // Arrange
        var createRequest = new
        {
            Name = "GET Integration Test Restaurant",
            Description = "Restaurant for GET integration testing.",
            Province = "Tehran",
            City = "Tehran",
            StreetAddress = "Valiasr Street",
            PostalCode = "1234567890"
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdRestaurant = await createResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        var restaurantId = createdRestaurant
            .GetProperty("id")
            .GetGuid();

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurantId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var restaurant = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            createRequest.Name,
            restaurant.GetProperty("name").GetString());

        Assert.Equal(
            createRequest.City,
            restaurant.GetProperty("city").GetString());
    }


    [Fact]
    public async Task GetById_WhenRestaurantDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurantId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task Update_WhenRestaurantExists_ReturnsNoContentAndUpdatesRestaurant()
    {
        // Arrange
        var createRequest = new
        {
            Name = "Original Restaurant",
            Description = "Original description",
            Province = "Tehran",
            City = "Tehran",
            StreetAddress = "Original Street",
            PostalCode = "1234567890"
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdRestaurant = await createResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        var restaurantId = createdRestaurant
            .GetProperty("id")
            .GetGuid();

        var updateRequest = new
        {
            Id = restaurantId,
            Name = "Updated Restaurant",
            Description = "Updated description",
            Province = "Isfahan",
            City = "Isfahan",
            StreetAddress = "Updated Street",
            PostalCode = "9876543210"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/restaurants/{restaurantId}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var updatedRestaurant = await dbContext.Restaurants
            .AsNoTracking()
            .SingleAsync(x => x.Id == restaurantId);

        Assert.Equal("Updated Restaurant", updatedRestaurant.Name);
        Assert.Equal("Isfahan", updatedRestaurant.Address.City);
    }


    [Fact]
    public async Task Deactivate_WhenRestaurantExists_ReturnsNoContentAndDeactivatesRestaurant()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                Name = "Status Test Restaurant",
                Description = "Status test",
                Province = "Tehran",
                City = "Tehran",
                StreetAddress = "Test Street",
                PostalCode = "1234567890"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        var restaurantId = created.GetProperty("id").GetGuid();

        // Act
        var response = await _client.PatchAsync(
            $"/api/restaurants/{restaurantId}/deactivate",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var restaurant = await dbContext.Restaurants
            .AsNoTracking()
            .SingleAsync(x => x.Id == restaurantId);

        Assert.False(restaurant.IsActive);
    }

    [Fact]
    public async Task Activate_WhenRestaurantExists_ReturnsNoContentAndActivatesRestaurant()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                Name = "Activate Test Restaurant",
                Description = "Activate test",
                Province = "Tehran",
                City = "Tehran",
                StreetAddress = "Test Street",
                PostalCode = "1234567890"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        var restaurantId = created.GetProperty("id").GetGuid();

        // Act
        var response = await _client.PatchAsync(
            $"/api/restaurants/{restaurantId}/deactivate",
            null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var activateResponse = await _client.PatchAsync(
            $"/api/restaurants/{restaurantId}/activate",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var restaurant = await dbContext.Restaurants
            .AsNoTracking()
            .SingleAsync(x => x.Id == restaurantId);

        Assert.True(restaurant.IsActive);
    }

}
