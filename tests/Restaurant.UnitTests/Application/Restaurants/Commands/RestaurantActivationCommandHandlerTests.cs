using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Application.Restaurants.Commands;

public sealed class RestaurantActivationCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;

    public RestaurantActivationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();
    }

    [Fact]
    public async Task Activate_WhenRestaurantExists_ShouldActivateAndSave()
    {
        // Arrange
        var restaurant = new RestaurantEntity(
            "Italian Restaurant",
            new RestaurantAddress("Tehran", "Tehran", "Valiasr Street"));

        restaurant.Deactivate();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var handler = new ActivateRestaurantCommandHandler(
            _repositoryMock.Object);

        // Act
        var result = await handler.Handle(
            new ActivateRestaurantCommand(restaurant.Id),
            CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(restaurant.IsActive);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Activate_WhenRestaurantDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var handler = new ActivateRestaurantCommandHandler(
            _repositoryMock.Object);

        // Act
        var result = await handler.Handle(
            new ActivateRestaurantCommand(restaurantId),
            CancellationToken.None);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Deactivate_WhenRestaurantExists_ShouldDeactivateAndSave()
    {
        // Arrange
        var restaurant = new RestaurantEntity(
            "Italian Restaurant",
            new RestaurantAddress("Tehran", "Tehran", "Valiasr Street"));

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var handler = new DeactivateRestaurantCommandHandler(
            _repositoryMock.Object);

        // Act
        var result = await handler.Handle(
            new DeactivateRestaurantCommand(restaurant.Id),
            CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.False(restaurant.IsActive);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Deactivate_WhenRestaurantDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var handler = new DeactivateRestaurantCommandHandler(
            _repositoryMock.Object);

        // Act
        var result = await handler.Handle(
            new DeactivateRestaurantCommand(restaurantId),
            CancellationToken.None);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}