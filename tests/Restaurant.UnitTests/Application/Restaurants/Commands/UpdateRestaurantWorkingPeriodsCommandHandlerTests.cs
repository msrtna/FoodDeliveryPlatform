using Moq;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.Features.Restaurants.Commands;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.UnitTests.Application.Features.Restaurants.Commands;

public sealed class UpdateRestaurantWorkingPeriodsCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock = new();

    [Fact]
    public async Task Handle_WhenRestaurantExists_UpdatesWorkingPeriods()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        var command = new UpdateRestaurantWorkingPeriodsCommand(
            restaurant.Id,
            [
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(14, 0)),
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(17, 0),
                    new TimeOnly(22, 0)),
                new WorkingPeriodInput(
                    DayOfWeek.Wednesday,
                    new TimeOnly(10, 0),
                    new TimeOnly(18, 0))
            ]);

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
        Assert.True(result);
        Assert.Equal(3, restaurant.WorkingPeriods.Count);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        var command = new UpdateRestaurantWorkingPeriodsCommand(
            restaurantId,
            []);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                restaurantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPeriodsOverlap_ThrowsDomainException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        var command = new UpdateRestaurantWorkingPeriodsCommand(
            restaurant.Id,
            [
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(14, 0)),
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(13, 0),
                    new TimeOnly(18, 0))
            ]);

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

    [Fact]
    public async Task Handle_WhenPeriodsAreEmpty_ClearsWorkingPeriods()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        restaurant.UpdateWorkingPeriods(
        [
            new WorkingPeriod(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(17, 0))
        ]);

        var command = new UpdateRestaurantWorkingPeriodsCommand(
            restaurant.Id,
            []);

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
        Assert.True(result);
        Assert.Empty(restaurant.WorkingPeriods);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
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

    private UpdateRestaurantWorkingPeriodsCommandHandler CreateHandler()
    {
        return new UpdateRestaurantWorkingPeriodsCommandHandler(
            _repositoryMock.Object);
    }
}