using Restaurant.Domain.Entities;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;


namespace Restaurant.UnitTests.Domain;

public sealed class RestaurantMenuCategoryTests
{
    [Fact]
    public void AddMenuCategory_WithValidCategory_AddsCategory()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var category = CreateCategory(restaurant.Id, "Pizza");

        // Act
        restaurant.AddMenuCategory(category);

        // Assert
        Assert.Single(restaurant.MenuCategories);
        Assert.Contains(category, restaurant.MenuCategories);
    }

    [Fact]
    public void AddMenuCategory_WhenCategoryBelongsToAnotherRestaurant_ThrowsDomainException()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var category = CreateCategory(Guid.NewGuid(), "Pizza");

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            restaurant.AddMenuCategory(category));

        Assert.Empty(restaurant.MenuCategories);
    }

    [Fact]
    public void AddMenuCategory_WhenNameAlreadyExists_ThrowsDomainException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        var firstCategory = CreateCategory(restaurant.Id, "Pizza");
        var secondCategory = CreateCategory(restaurant.Id, "  PIZZA  ");

        restaurant.AddMenuCategory(firstCategory);

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            restaurant.AddMenuCategory(secondCategory));

        Assert.Single(restaurant.MenuCategories);
    }

    [Fact]
    public void AddMenuCategory_WithNullCategory_ThrowsArgumentNullException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            restaurant.AddMenuCategory(null!));
    }

    [Fact]
    public void RemoveMenuCategory_WhenCategoryExists_RemovesCategory()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var category = CreateCategory(restaurant.Id, "Pizza");

        restaurant.AddMenuCategory(category);

        // Act
        restaurant.RemoveMenuCategory(category.Id);

        // Assert
        Assert.Empty(restaurant.MenuCategories);
    }

    [Fact]
    public void RemoveMenuCategory_WhenCategoryDoesNotExist_ThrowsDomainException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            restaurant.RemoveMenuCategory(Guid.NewGuid()));
    }

    private static RestaurantEntity CreateRestaurant()
    {
        return new RestaurantEntity(
            "Test Restaurant",
            new RestaurantAddress(
                "Tehran",
                "Tehran",
                "Valiasr Street"));
    }

    private static MenuCategory CreateCategory(
        Guid restaurantId,
        string name)
    {
        return new MenuCategory(restaurantId, name);
    }
}