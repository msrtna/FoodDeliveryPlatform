using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.IntegrationTests.Infrastructure;
using Restaurant.Infrastructure.Persistence.Context;
using Xunit;

namespace Restaurant.IntegrationTests.MenuCategories;

public sealed class CreateMenuCategoryTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CreateMenuCategoryTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAndPersistsCategory()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Category Test Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        Assert.Equal(HttpStatusCode.Created, restaurantResponse.StatusCode);

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var request = new
        {
            name = "Main Dishes",
            description = "All main dishes",
            displayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var category = await response.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);
        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(request.name, category.Name);
        Assert.Equal(request.description, category.Description);
        Assert.Equal(request.displayOrder, category.DisplayOrder);

        Assert.NotNull(response.Headers.Location);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var persistedCategory = await dbContext.MenuCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == category.Id);

        Assert.NotNull(persistedCategory);
        Assert.Equal(restaurant.Id, persistedCategory.RestaurantId);
        Assert.Equal(request.name, persistedCategory.Name);
    }

    [Fact]
    public async Task GetById_WhenCategoryExists_ReturnsCategory()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Get Category Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var createCategoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Burgers",
                description = "Burger menu",
                displayOrder = 2
            });

        Assert.Equal(HttpStatusCode.Created, createCategoryResponse.StatusCode);

        var createdCategory = await createCategoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(createdCategory);

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{createdCategory.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(result);
        Assert.Equal(createdCategory.Id, result.Id);
        Assert.Equal("Burgers", result.Name);
        Assert.Equal("Burger menu", result.Description);
        Assert.Equal(2, result.DisplayOrder);
    }

    [Fact]
    public async Task GetByRestaurantId_WhenCategoriesExist_ReturnsCategoriesOrderedByDisplayOrder()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Category List Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        foreach (var category in new[]
        {
        new { name = "Desserts", displayOrder = 3 },
        new { name = "Starters", displayOrder = 1 },
        new { name = "Main Courses", displayOrder = 2 }
    })
        {
            var createResponse = await _client.PostAsJsonAsync(
                $"/api/restaurants/{restaurant.Id}/menu-categories",
                new
                {
                    category.name,
                    description = (string?)null,
                    category.displayOrder
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<MenuCategoryResponse>>();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal("Starters", result[0].Name);
        Assert.Equal("Main Courses", result[1].Name);
        Assert.Equal("Desserts", result[2].Name);
    }

    [Fact]
    public async Task GetByRestaurantId_WithIsActiveFilter_ReturnsOnlyActiveCategories()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Active Category Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var createdCategories = new List<MenuCategoryResponse>();

        foreach (var name in new[] { "Active Category", "Inactive Category" })
        {
            var createResponse = await _client.PostAsJsonAsync(
                $"/api/restaurants/{restaurant.Id}/menu-categories",
                new
                {
                    name,
                    description = (string?)null,
                    displayOrder = 0
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var category = await createResponse.Content
                .ReadFromJsonAsync<MenuCategoryResponse>();

            Assert.NotNull(category);
            createdCategories.Add(category);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<RestaurantDbContext>();

            var inactiveCategory = await dbContext.MenuCategories
                .FirstAsync(x => x.Id == createdCategories[1].Id);

            inactiveCategory.Deactivate();

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories?isActive=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<MenuCategoryResponse>>();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Active Category", result[0].Name);
    }

    [Fact]
    public async Task GetByRestaurantId_WithIsActiveFalse_ReturnsOnlyInactiveCategories()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Inactive Category Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var createdCategories = new List<MenuCategoryResponse>();

        foreach (var name in new[] { "Active Category", "Inactive Category" })
        {
            var createResponse = await _client.PostAsJsonAsync(
                $"/api/restaurants/{restaurant.Id}/menu-categories",
                new
                {
                    name,
                    description = (string?)null,
                    displayOrder = 0
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var category = await createResponse.Content
                .ReadFromJsonAsync<MenuCategoryResponse>();

            Assert.NotNull(category);
            createdCategories.Add(category);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<RestaurantDbContext>();

            var inactiveCategory = await dbContext.MenuCategories
                .FirstAsync(x => x.Id == createdCategories[1].Id);

            inactiveCategory.Deactivate();

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories?isActive=false");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<MenuCategoryResponse>>();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Inactive Category", result[0].Name);
    }

    private sealed record RestaurantResponse(Guid Id);

    private sealed record MenuCategoryResponse(
        Guid Id,
        string Name,
        string? Description,
        int DisplayOrder);
}
