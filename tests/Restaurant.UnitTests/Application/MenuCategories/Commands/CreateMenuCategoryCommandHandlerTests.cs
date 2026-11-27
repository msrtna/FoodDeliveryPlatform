using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.MenuCategories.Commands;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Application.MenuCategories.Commands;

public sealed class CreateMenuCategoryCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock = new();

    [Fact]
    public async Task Handle_WhenRestaurantExists_CreatesMenuCategory()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        var command = new CreateMenuCategoryCommand(
            restaurant.Id,
            "Pizza",
            "Different types of pizza",
            1);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(restaurant.Id, result.RestaurantId);
        Assert.Equal("Pizza", result.Name);
        Assert.Equal("Different types of pizza", result.Description);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsActive);

        Assert.Single(restaurant.MenuCategories);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        var command = new CreateMenuCategoryCommand(
            restaurantId,
            "Pizza",
            null,
            0);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCategoryNameAlreadyExists_ThrowsDomainException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        restaurant.AddMenuCategory(
            new MenuCategory(restaurant.Id, "Pizza"));

        var command = new CreateMenuCategoryCommand(
            restaurant.Id,
            "  PIZZA  ",
            null,
            1);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
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

    private CreateMenuCategoryCommandHandler CreateHandler()
    {
        return new CreateMenuCategoryCommandHandler(
            _repositoryMock.Object);
    }
}