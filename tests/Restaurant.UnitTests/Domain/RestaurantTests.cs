using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;
using Restaurant.Domain.Exceptions;

namespace Restaurant.UnitTests.Domain;

public sealed class RestaurantTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesActiveRestaurant()
    {
        // Arrange
        const string name = "  Pizza House  ";
        const string description = "  Fresh pizza  ";

        // Act
        var restaurant = new RestaurantEntity(name, description);

        // Assert
        Assert.NotEqual(Guid.Empty, restaurant.Id);
        Assert.Equal("Pizza House", restaurant.Name);
        Assert.Equal("Fresh pizza", restaurant.Description);
        Assert.True(restaurant.IsActive);
        Assert.NotEqual(default, restaurant.CreatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ThrowsDomainException(string name)
    {
        // Act
        var action = () => new RestaurantEntity(name);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_WithNameLongerThan150Characters_ThrowsDomainException()
    {
        // Arrange
        var name = new string('A', 151);

        // Act
        var action = () => new RestaurantEntity(name);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_WithDescriptionLongerThan1000Characters_ThrowsDomainException()
    {
        // Arrange
        var description = new string('A', 1001);

        // Act
        var action = () => new RestaurantEntity("Pizza House", description);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void UpdateInfo_WithValidData_UpdatesNameAndDescription()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Old Name", "Old Description");

        // Act
        restaurant.UpdateInfo("  New Name  ", "  New Description  ");

        // Assert
        Assert.Equal("New Name", restaurant.Name);
        Assert.Equal("New Description", restaurant.Description);
    }

    [Fact]
    public void UpdateInfo_WithInvalidDescription_DoesNotChangeExistingData()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Old Name", "Old Description");
        var invalidDescription = new string('A', 1001);

        // Act
        var action = () => restaurant.UpdateInfo("New Name", invalidDescription);

        // Assert
        Assert.Throws<DomainException>(action);
        Assert.Equal("Old Name", restaurant.Name);
        Assert.Equal("Old Description", restaurant.Description);
    }

    [Fact]
    public void UpdateInfo_WithNullDescription_ClearsDescription()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Pizza House", "Old Description");

        // Act
        restaurant.UpdateInfo("Pizza House", null);

        // Assert
        Assert.Null(restaurant.Description);
    }

    [Fact]
    public void Deactivate_ChangesIsActiveToFalse()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Pizza House");

        // Act
        restaurant.Deactivate();

        // Assert
        Assert.False(restaurant.IsActive);
    }

    [Fact]
    public void Activate_ChangesIsActiveToTrue()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Pizza House");
        restaurant.Deactivate();

        // Act
        restaurant.Activate();

        // Assert
        Assert.True(restaurant.IsActive);
    }
}