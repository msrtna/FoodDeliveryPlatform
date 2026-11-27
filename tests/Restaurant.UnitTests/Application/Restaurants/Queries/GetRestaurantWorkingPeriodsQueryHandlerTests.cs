using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.Restaurants.Queries;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.UnitTests.Application.Features.Restaurants.Queries;

public sealed class GetRestaurantWorkingPeriodsQueryHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock = new();

    [Fact]
    public async Task Handle_WhenRestaurantExists_ReturnsOrderedWorkingPeriods()
    {
        // Arrange
        var restaurant = new RestaurantEntity(
            "Test Restaurant",
            new RestaurantAddress(
                "Tehran",
                "Tehran",
                "Valiasr Street"));

        restaurant.UpdateWorkingPeriods(
        [
            new WorkingPeriod(
                DayOfWeek.Monday,
                new TimeOnly(17, 0),
                new TimeOnly(22, 0)),
            new WorkingPeriod(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(14, 0)),
            new WorkingPeriod(
                DayOfWeek.Wednesday,
                new TimeOnly(10, 0),
                new TimeOnly(18, 0))
        ]);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var handler = new GetRestaurantWorkingPeriodsQueryHandler(
            _repositoryMock.Object);

        var query = new GetRestaurantWorkingPeriodsQuery(restaurant.Id);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal(DayOfWeek.Monday, result.ElementAt(0).DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), result.ElementAt(0).OpenTime);

        Assert.Equal(DayOfWeek.Monday, result.ElementAt(1).DayOfWeek);
        Assert.Equal(new TimeOnly(17, 0), result.ElementAt(1).OpenTime);

        Assert.Equal(DayOfWeek.Wednesday, result.ElementAt(2).DayOfWeek);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantDoesNotExist_ReturnsNull()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var handler = new GetRestaurantWorkingPeriodsQueryHandler(
            _repositoryMock.Object);

        var query = new GetRestaurantWorkingPeriodsQuery(restaurantId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantHasNoWorkingPeriods_ReturnsEmptyCollection()
    {
        // Arrange
        var restaurant = new RestaurantEntity(
            "Test Restaurant",
            new RestaurantAddress(
                "Tehran",
                "Tehran",
                "Valiasr Street"));

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurant.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var handler = new GetRestaurantWorkingPeriodsQueryHandler(
            _repositoryMock.Object);

        var query = new GetRestaurantWorkingPeriodsQuery(restaurant.Id);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}