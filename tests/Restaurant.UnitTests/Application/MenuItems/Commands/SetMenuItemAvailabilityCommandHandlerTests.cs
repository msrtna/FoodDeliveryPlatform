using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuItems.Commands;
using Restaurant.Domain.Entities;
using Xunit;

namespace Restaurant.UnitTests.Application.MenuItems.Commands;

public sealed class SetMenuItemAvailabilityCommandHandlerTests
{
    private readonly Mock<IMenuCategoryRepository> _menuCategoryRepositoryMock = new();
    private readonly Mock<IRestaurantRepository> _restaurantRepositoryMock = new();

    private SetMenuItemAvailabilityCommandHandler CreateSut()
    {
        return new SetMenuItemAvailabilityCommandHandler(
            _menuCategoryRepositoryMock.Object,
            _restaurantRepositoryMock.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_WhenMenuItemExists_UpdatesAvailabilityAndSavesChanges(
        bool isAvailable)
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var category = new MenuCategory(restaurantId, "Burgers");

        var menuItem = new MenuItem(
            category.Id,
            "Classic Burger",
            12.50m);

        category.AddMenuItem(menuItem);

        if (!isAvailable)
            menuItem.SetUnavailable();

        var command = new SetMenuItemAvailabilityCommand(
            restaurantId,
            category.Id,
            menuItem.Id,
            isAvailable);

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
        Assert.Equal(isAvailable, result.IsAvailable);
        Assert.Equal(menuItem.Id, result.Id);

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

        var command = new SetMenuItemAvailabilityCommand(
            restaurantId,
            categoryId,
            Guid.NewGuid(),
            false);

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

        var command = new SetMenuItemAvailabilityCommand(
            restaurantId,
            category.Id,
            Guid.NewGuid(),
            false);

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
}