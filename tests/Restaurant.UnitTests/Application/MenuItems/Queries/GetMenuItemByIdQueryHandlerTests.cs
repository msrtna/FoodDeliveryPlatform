using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuItems.Queries;
using Restaurant.Domain.Entities;

namespace Restaurant.UnitTests.Application.MenuItems.Queries;

public sealed class GetMenuItemByIdQueryHandlerTests
{
    private readonly Mock<IMenuCategoryRepository> _menuCategoryRepositoryMock = new();
    private readonly Mock<IMenuItemRepository> _menuItemRepositoryMock = new();

    private GetMenuItemByIdQueryHandler CreateSut()
    {
        return new GetMenuItemByIdQueryHandler(
            _menuCategoryRepositoryMock.Object,
            _menuItemRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMenuItemExists_ReturnsMenuItemDto()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");
        var menuItem = new MenuItem(
            category.Id,
            "Classic Burger",
            12.50m,
            "Beef burger",
            1);

        var query = new GetMenuItemByIdQuery(
            restaurantId,
            category.Id,
            menuItem.Id);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _menuItemRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                category.Id,
                menuItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuItem);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(menuItem.Id, result.Id);
        Assert.Equal(category.Id, result.MenuCategoryId);
        Assert.Equal("Classic Burger", result.Name);
        Assert.Equal("Beef burger", result.Description);
        Assert.Equal(12.50m, result.Price);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsAvailable);

        _menuCategoryRepositoryMock.Verify(
            repository => repository.GetByIdAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _menuItemRepositoryMock.Verify(
            repository => repository.GetByIdAsync(
                category.Id,
                menuItem.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();

        var query = new GetMenuItemByIdQuery(
            restaurantId,
            categoryId,
            menuItemId);

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
            repository => repository.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMenuItemDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");
        var menuItemId = Guid.NewGuid();

        var query = new GetMenuItemByIdQuery(
            restaurantId,
            category.Id,
            menuItemId);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _menuItemRepositoryMock
            .Setup(repository => repository.GetByIdAsync(
                category.Id,
                menuItemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuItem?)null);

        var sut = CreateSut();

        // Act
        var act = () => sut.Handle(query, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _menuItemRepositoryMock.Verify(
            repository => repository.GetByIdAsync(
                category.Id,
                menuItemId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}