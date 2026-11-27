using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuCategories.Queries;
using Restaurant.Domain.Entities;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Application.MenuCategories.Queries;

public sealed class GetMenuCategoriesByRestaurantIdQueryHandlerTests
{
    private readonly Mock<IRestaurantRepository> _restaurantRepositoryMock = new();
    private readonly Mock<IMenuCategoryRepository> _menuCategoryRepositoryMock = new();

    [Fact]
    public async Task Handle_WhenRestaurantExists_ReturnsItsMenuCategories()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        var categories = new List<MenuCategory>
        {
            new(restaurant.Id, "Pizza", "Pizza menu", 1),
            new(restaurant.Id, "Drinks", "Beverages", 2)
        };

        var query = new GetMenuCategoriesByRestaurantIdQuery(
            restaurant.Id);

        _restaurantRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByRestaurantIdAsync(
                restaurant.Id,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            category =>
            {
                Assert.Equal("Pizza", category.Name);
                Assert.Equal(1, category.DisplayOrder);
                Assert.Equal(restaurant.Id, category.RestaurantId);
            },
            category =>
            {
                Assert.Equal("Drinks", category.Name);
                Assert.Equal(2, category.DisplayOrder);
                Assert.Equal(restaurant.Id, category.RestaurantId);
            });

        _menuCategoryRepositoryMock.Verify(
            repository => repository.GetByRestaurantIdAsync(
                restaurant.Id,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        var query = new GetMenuCategoriesByRestaurantIdQuery(
            restaurantId);

        _restaurantRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));

        _menuCategoryRepositoryMock.Verify(
            repository => repository.GetByRestaurantIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenIsActiveFilterIsProvided_PassesFilterToRepository()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        var query = new GetMenuCategoriesByRestaurantIdQuery(
            restaurant.Id,
            IsActive: true);

        _restaurantRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByRestaurantIdAsync(
                restaurant.Id,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MenuCategory>());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Empty(result);

        _menuCategoryRepositoryMock.Verify(
            repository => repository.GetByRestaurantIdAsync(
                restaurant.Id,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
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

    private GetMenuCategoriesByRestaurantIdQueryHandler CreateHandler()
    {
        return new GetMenuCategoriesByRestaurantIdQueryHandler(
            _restaurantRepositoryMock.Object,
            _menuCategoryRepositoryMock.Object);
    }
}
