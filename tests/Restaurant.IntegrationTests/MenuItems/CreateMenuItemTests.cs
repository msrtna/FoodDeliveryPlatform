using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.IntegrationTests.Infrastructure;
using Restaurant.Infrastructure.Persistence.Context;
using Xunit;

namespace Restaurant.IntegrationTests.MenuItems;

public sealed class CreateMenuItemTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CreateMenuItemTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAndPersistsMenuItem()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Menu Item Test Restaurant",
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

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Main Courses",
                description = "Main course category",
                displayOrder = 1
            });

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        var request = new
        {
            name = "Cheeseburger",
            price = 12.50m,
            description = "Beef burger with cheese",
            displayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var menuItem = await response.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(menuItem);
        Assert.NotEqual(Guid.Empty, menuItem.Id);
        Assert.Equal(category.Id, menuItem.MenuCategoryId);
        Assert.Equal(request.name, menuItem.Name);
        Assert.Equal(request.description, menuItem.Description);
        Assert.Equal(request.price, menuItem.Price);
        Assert.Equal(request.displayOrder, menuItem.DisplayOrder);
        Assert.True(menuItem.IsAvailable);

        Assert.NotNull(response.Headers.Location);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var persistedMenuItem = await dbContext.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == menuItem.Id);

        Assert.NotNull(persistedMenuItem);
        Assert.Equal(category.Id, persistedMenuItem.MenuCategoryId);
        Assert.Equal(request.name, persistedMenuItem.Name);
        Assert.Equal(request.price, persistedMenuItem.Price);
    }

    [Fact]
    public async Task GetById_WhenMenuItemExists_ReturnsMenuItem()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Get Menu Item Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Burgers",
                description = "Burger category",
                displayOrder = 1
            });

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
            new
            {
                name = "Classic Burger",
                price = 15.50m,
                description = "Classic beef burger",
                displayOrder = 1
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdMenuItem = await createResponse.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(createdMenuItem);

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items/{createdMenuItem.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(result);
        Assert.Equal(createdMenuItem.Id, result.Id);
        Assert.Equal(category.Id, result.MenuCategoryId);
        Assert.Equal("Classic Burger", result.Name);
        Assert.Equal("Classic beef burger", result.Description);
        Assert.Equal(15.50m, result.Price);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsAvailable);
    }

    [Fact]
    public async Task GetByCategory_WhenMenuItemsExist_ReturnsItemsOrderedByDisplayOrder()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Menu Item List Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Main Courses",
                description = "Main course category",
                displayOrder = 1
            });

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        foreach (var item in new[]
        {
        new { name = "Pizza", price = 18m, displayOrder = 3 },
        new { name = "Burger", price = 15m, displayOrder = 1 },
        new { name = "Pasta", price = 16m, displayOrder = 2 }
    })
        {
            var createResponse = await _client.PostAsJsonAsync(
                $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
                new
                {
                    item.name,
                    item.price,
                    description = (string?)null,
                    item.displayOrder
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<MenuItemResponse>>();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal("Burger", result[0].Name);
        Assert.Equal("Pasta", result[1].Name);
        Assert.Equal("Pizza", result[2].Name);
    }

    [Fact]
    public async Task GetByCategory_WithIsAvailableTrue_ReturnsOnlyAvailableItems()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Available Items Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Main Courses",
                description = "Main course category",
                displayOrder = 1
            });

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        var createdItems = new List<MenuItemResponse>();

        foreach (var name in new[] { "Available Burger", "Unavailable Pizza" })
        {
            var createResponse = await _client.PostAsJsonAsync(
                $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
                new
                {
                    name,
                    price = 15m,
                    description = (string?)null,
                    displayOrder = 1
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var item = await createResponse.Content
                .ReadFromJsonAsync<MenuItemResponse>();

            Assert.NotNull(item);
            createdItems.Add(item);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<RestaurantDbContext>();

            var unavailableItem = await dbContext.MenuItems
                .FirstAsync(x => x.Id == createdItems[1].Id);

            unavailableItem.SetUnavailable();

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items?isAvailable=true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<MenuItemResponse>>();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Available Burger", result[0].Name);
    }

    [Fact]
    public async Task GetByCategory_WithIsAvailableFalse_ReturnsOnlyUnavailableItems()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Unavailable Items Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Main Courses",
                description = "Main course category",
                displayOrder = 1
            });

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        var createdItems = new List<MenuItemResponse>();

        foreach (var name in new[] { "Available Burger", "Unavailable Pizza" })
        {
            var createResponse = await _client.PostAsJsonAsync(
                $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
                new
                {
                    name,
                    price = 15m,
                    description = (string?)null,
                    displayOrder = 1
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var item = await createResponse.Content
                .ReadFromJsonAsync<MenuItemResponse>();

            Assert.NotNull(item);
            createdItems.Add(item);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<RestaurantDbContext>();

            var unavailableItem = await dbContext.MenuItems
                .FirstAsync(x => x.Id == createdItems[1].Id);

            unavailableItem.SetUnavailable();

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items?isAvailable=false");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<MenuItemResponse>>();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Unavailable Pizza", result[0].Name);
        Assert.False(result[0].IsAvailable);
    }

    [Fact]
    public async Task Update_WhenMenuItemExists_ReturnsOkAndUpdatesMenuItem()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Update Menu Item Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Main Courses",
                description = "Main course category",
                displayOrder = 1
            });

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
            new
            {
                name = "Old Burger",
                price = 10m,
                description = "Old description",
                displayOrder = 1
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdItem = await createResponse.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(createdItem);

        var updateRequest = new
        {
            name = "Premium Burger",
            price = 18.50m,
            description = "Updated description",
            displayOrder = 2
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items/{createdItem.Id}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedItem = await updateResponse.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(updatedItem);
        Assert.Equal(createdItem.Id, updatedItem.Id);
        Assert.Equal(category.Id, updatedItem.MenuCategoryId);
        Assert.Equal(updateRequest.name, updatedItem.Name);
        Assert.Equal(updateRequest.price, updatedItem.Price);
        Assert.Equal(updateRequest.description, updatedItem.Description);
        Assert.Equal(updateRequest.displayOrder, updatedItem.DisplayOrder);
        Assert.True(updatedItem.IsAvailable);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var persistedItem = await dbContext.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == createdItem.Id);

        Assert.NotNull(persistedItem);
        Assert.Equal(updateRequest.name, persistedItem.Name);
        Assert.Equal(updateRequest.price, persistedItem.Price);
        Assert.Equal(updateRequest.description, persistedItem.Description);
        Assert.Equal(updateRequest.displayOrder, persistedItem.DisplayOrder);
    }

    [Fact]
    public async Task SetAvailability_WhenMenuItemExists_ReturnsOkAndUpdatesAvailability()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Availability Test Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Main Courses",
                description = "Main course category",
                displayOrder = 1
            });

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
            new
            {
                name = "Availability Burger",
                price = 15m,
                description = "Integration test",
                displayOrder = 1
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdItem = await createResponse.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(createdItem);
        Assert.True(createdItem.IsAvailable);

        // Act
        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items/{createdItem.Id}/availability",
            new
            {
                isAvailable = false
            });

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedItem = await updateResponse.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(updatedItem);
        Assert.Equal(createdItem.Id, updatedItem.Id);
        Assert.False(updatedItem.IsAvailable);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var persistedItem = await dbContext.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == createdItem.Id);

        Assert.NotNull(persistedItem);
        Assert.False(persistedItem.IsAvailable);
    }

    [Fact]
    public async Task SetAvailability_WhenMenuItemIsUnavailable_ActivatesMenuItem()
    {
        // Arrange
        var restaurantResponse = await _client.PostAsJsonAsync(
            "/api/restaurants",
            new
            {
                name = "Activate Menu Item Restaurant",
                description = "Integration test",
                province = "Tehran",
                city = "Tehran",
                streetAddress = "Valiasr Street",
                postalCode = "1234567890"
            });

        var restaurant = await restaurantResponse.Content
            .ReadFromJsonAsync<RestaurantResponse>();

        Assert.NotNull(restaurant);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories",
            new
            {
                name = "Main Courses",
                description = "Main course category",
                displayOrder = 1
            });

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<MenuCategoryResponse>();

        Assert.NotNull(category);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items",
            new
            {
                name = "Activate Burger",
                price = 15m,
                description = "Integration test",
                displayOrder = 1
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdItem = await createResponse.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(createdItem);

        var deactivateResponse = await _client.PatchAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items/{createdItem.Id}/availability",
            new { isAvailable = false });

        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        // Act
        var activateResponse = await _client.PatchAsJsonAsync(
            $"/api/restaurants/{restaurant.Id}/menu-categories/{category.Id}/menu-items/{createdItem.Id}/availability",
            new { isAvailable = true });

        // Assert
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);

        var activatedItem = await activateResponse.Content
            .ReadFromJsonAsync<MenuItemResponse>();

        Assert.NotNull(activatedItem);
        Assert.Equal(createdItem.Id, activatedItem.Id);
        Assert.True(activatedItem.IsAvailable);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RestaurantDbContext>();

        var persistedItem = await dbContext.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == createdItem.Id);

        Assert.NotNull(persistedItem);
        Assert.True(persistedItem.IsAvailable);
    }

    private sealed record RestaurantResponse(Guid Id);

    private sealed record MenuCategoryResponse(Guid Id);

    private sealed record MenuItemResponse(
        Guid Id,
        Guid MenuCategoryId,
        string Name,
        string? Description,
        decimal Price,
        int DisplayOrder,
        bool IsAvailable);
}
