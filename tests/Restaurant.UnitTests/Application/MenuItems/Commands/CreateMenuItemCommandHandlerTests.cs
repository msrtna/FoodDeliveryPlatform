using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuItems.Commands;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;


namespace Restaurant.UnitTests.Application.MenuItems.Commands;

public sealed class CreateMenuItemCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _restaurantRepository = new();
    private readonly Mock<IMenuCategoryRepository> _menuCategoryRepository = new();

    private readonly CreateMenuItemCommandHandler _handler;

    public CreateMenuItemCommandHandlerTests()
    {
        _handler = new CreateMenuItemCommandHandler(
            _restaurantRepository.Object,
            _menuCategoryRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesMenuItem()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var category = CreateCategory(restaurant.Id);

        var command = new CreateMenuItemCommand(
            restaurant.Id,
            category.Id,
            "Margherita",
            12.50m,
            "Classic cheese pizza",
            1);

        _restaurantRepository
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _menuCategoryRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                restaurant.Id,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _restaurantRepository
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(category.Id, result.MenuCategoryId);
        Assert.Equal("Margherita", result.Name);
        Assert.Equal(12.50m, result.Price);
        Assert.Equal("Classic cheese pizza", result.Description);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsAvailable);

        Assert.Single(category.MenuItems);
        Assert.Equal(result.Id, category.MenuItems.First().Id);

        _restaurantRepository.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        var command = new CreateMenuItemCommand(
            restaurantId,
            Guid.NewGuid(),
            "Margherita",
            12.50m,
            null,
            1);

        _restaurantRepository
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        // Act
        var act = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _menuCategoryRepository.Verify(
            repository => repository.GetByIdForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _restaurantRepository.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var categoryId = Guid.NewGuid();

        var command = new CreateMenuItemCommand(
            restaurant.Id,
            categoryId,
            "Margherita",
            12.50m,
            null,
            1);

        _restaurantRepository
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _menuCategoryRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                restaurant.Id,
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        // Act
        var act = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _restaurantRepository.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDuplicateItemNameExists_ThrowsDomainException()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var category = CreateCategory(restaurant.Id);

        category.AddMenuItem(
            new MenuItem(category.Id, "Margherita", 10m));

        var command = new CreateMenuItemCommand(
            restaurant.Id,
            category.Id,
            "margherita",
            12.50m,
            null,
            2);

        _restaurantRepository
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _menuCategoryRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                restaurant.Id,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var act = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        Assert.Single(category.MenuItems);

        _restaurantRepository.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static RestaurantEntity CreateRestaurant()
    {
        var address = new RestaurantAddress(
        "Baku",
        "Baku",
        "Nizami Street",
        "AZ1000");

        return new RestaurantEntity(
            "Test Restaurant",
            address);
    }

    private static MenuCategory CreateCategory(Guid restaurantId)
    {
        return new MenuCategory(
            restaurantId,
            "Pizza");
    }
}