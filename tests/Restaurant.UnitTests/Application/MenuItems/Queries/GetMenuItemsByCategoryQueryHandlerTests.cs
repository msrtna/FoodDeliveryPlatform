using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuItems.Queries;
using Restaurant.Domain.Entities;
using Xunit;

namespace Restaurant.UnitTests.Application.MenuItems.Queries;

public sealed class GetMenuItemsByCategoryQueryHandlerTests
{
    private readonly Mock<IMenuCategoryRepository> _menuCategoryRepositoryMock = new();
    private readonly Mock<IMenuItemRepository> _menuItemRepositoryMock = new();

    private GetMenuItemsByCategoryQueryHandler CreateSut()
    {
        return new GetMenuItemsByCategoryQueryHandler(
            _menuCategoryRepositoryMock.Object,
            _menuItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_ReturnsMenuItems()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");

        var availableItem = new MenuItem(
            category.Id,
            "Classic Burger",
            12.50m,
            "Beef burger",
            1);

        var unavailableItem = new MenuItem(
            category.Id,
            "Chicken Burger",
            10m,
            "Chicken burger",
            2);

        unavailableItem.SetUnavailable();

        var query = new GetMenuItemsByCategoryQuery(
            restaurantId,
            category.Id);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _menuItemRepositoryMock
            .Setup(repository => repository.GetByCategoryIdAsync(
                category.Id,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { availableItem, unavailableItem });

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, item => item.Name == "Classic Burger");
        Assert.Contains(result, item => item.Name == "Chicken Burger");

        _menuItemRepositoryMock.Verify(
            repository => repository.GetByCategoryIdAsync(
                category.Id,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAvailabilityFilterIsProvided_PassesFilterToRepository()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");

        var query = new GetMenuItemsByCategoryQuery(
            restaurantId,
            category.Id,
            IsAvailable: true);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _menuItemRepositoryMock
            .Setup(repository => repository.GetByCategoryIdAsync(
                category.Id,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MenuItem>());

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);

        _menuItemRepositoryMock.Verify(
            repository => repository.GetByCategoryIdAsync(
                category.Id,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var query = new GetMenuItemsByCategoryQuery(
            restaurantId,
            categoryId);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        var sut = CreateSut();

        // Act
        var act = () => sut.Handle(query, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _menuItemRepositoryMock.Verify(
            repository => repository.GetByCategoryIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}