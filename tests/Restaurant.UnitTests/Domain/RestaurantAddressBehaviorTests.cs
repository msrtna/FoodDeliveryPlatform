using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Domain;

public sealed class RestaurantAddressIntegrationTests
{
    [Fact]
    public void Constructor_WithAddress_SetsAddress()
    {
        // Arrange
        var address = new RestaurantAddress(
            "Tehran Province",
            "Tehran",
            "Valiasr Street");

        // Act
        var restaurant = new RestaurantEntity(
            "Test Restaurant",
            address);

        // Assert
        Assert.Equal(address, restaurant.Address);
    }

    [Fact]
    public void UpdateAddress_WithValidAddress_UpdatesAddress()
    {
        // Arrange
        var originalAddress = new RestaurantAddress(
            "Tehran Province",
            "Tehran",
            "Valiasr Street");

        var newAddress = new RestaurantAddress(
            "Isfahan Province",
            "Isfahan",
            "Chaharbagh Street");

        var restaurant = new RestaurantEntity(
            "Test Restaurant",
            originalAddress);

        // Act
        restaurant.UpdateAddress(newAddress);

        // Assert
        Assert.Equal(newAddress, restaurant.Address);
    }

    [Fact]
    public void UpdateAddress_WithNull_ThrowsAndPreservesOriginalAddress()
    {
        // Arrange
        var originalAddress = new RestaurantAddress(
            "Tehran Province",
            "Tehran",
            "Valiasr Street");

        var restaurant = new RestaurantEntity(
            "Test Restaurant",
            originalAddress);

        // Act
        var action = () => restaurant.UpdateAddress(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(originalAddress, restaurant.Address);
    }
}