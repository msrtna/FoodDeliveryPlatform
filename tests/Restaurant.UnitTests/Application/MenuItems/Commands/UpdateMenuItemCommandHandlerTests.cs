using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuItems.Commands;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Exceptions;
using Xunit;

namespace Restaurant.UnitTests.Application.MenuItems.Commands;

public sealed class UpdateMenuItemCommandHandlerTests
{
    private readonly Mock<IMenuCategoryRepository> _menuCategoryRepositoryMock = new();
    private readonly Mock<IRestaurantRepository> _restaurantRepositoryMock = new();

    private UpdateMenuItemCommandHandler CreateSut()
    {
        return new UpdateMenuItemCommandHandler(
            _menuCategoryRepositoryMock.Object,
            _restaurantRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMenuItemExists_UpdatesItemAndSavesChanges()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");
        var menuItem = new MenuItem(
            category.Id,
            "Classic Burger",
            12.50m,
            "Original description",
            1);

        category.AddMenuItem(menuItem);

        var command = new UpdateMenuItemCommand(
            restaurantId,
            category.Id,
            menuItem.Id,
            "Deluxe Burger",
            15m,
            "Updated description",
            2);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _restaurantRepositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(menuItem.Id, result.Id);
        Assert.Equal("Deluxe Burger", result.Name);
        Assert.Equal(15m, result.Price);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(2, result.DisplayOrder);

        _restaurantRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
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

        var command = new UpdateMenuItemCommand(
            restaurantId,
            categoryId,
            menuItemId,
            "Updated Item",
            10m,
            null,
            0);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                restaurantId,
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        var sut = CreateSut();

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _restaurantRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMenuItemDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");

        var command = new UpdateMenuItemCommand(
            restaurantId,
            category.Id,
            Guid.NewGuid(),
            "Updated Item",
            10m,
            null,
            0);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var sut = CreateSut();

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _restaurantRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ThrowsDomainException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");

        category.AddMenuItem(new MenuItem(
            category.Id,
            "Classic Burger",
            12m));

        var itemToUpdate = new MenuItem(
            category.Id,
            "Chicken Burger",
            10m);

        category.AddMenuItem(itemToUpdate);

        var command = new UpdateMenuItemCommand(
            restaurantId,
            category.Id,
            itemToUpdate.Id,
            " classic burger ",
            14m,
            null,
            1);

        _menuCategoryRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(
                restaurantId,
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var sut = CreateSut();

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        _restaurantRepositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}