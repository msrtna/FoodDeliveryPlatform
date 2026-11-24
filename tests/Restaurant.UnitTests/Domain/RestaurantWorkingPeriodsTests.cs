using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;
using RestaurantEntity = Restaurant.Domain.Entities.Restaurant;

namespace Restaurant.UnitTests.Domain;

public sealed class RestaurantWorkingPeriodsTests
{
    [Fact]
    public void UpdateWorkingPeriods_WithValidPeriods_UpdatesSchedule()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Test Restaurant");

        var periods = new List<WorkingPeriod>
        {
            new(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(14, 0)),
            new(
                DayOfWeek.Monday,
                new TimeOnly(17, 0),
                new TimeOnly(23, 0)),
            new(
                DayOfWeek.Tuesday,
                new TimeOnly(10, 0),
                new TimeOnly(22, 0))
        };

        // Act
        restaurant.UpdateWorkingPeriods(periods);

        // Assert
        Assert.Equal(3, restaurant.WorkingPeriods.Count);
        Assert.Contains(
            restaurant.WorkingPeriods,
            period => period.DayOfWeek == DayOfWeek.Tuesday);
    }

    [Fact]
    public void UpdateWorkingPeriods_WithOverlappingPeriods_ThrowsDomainException()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Test Restaurant");

        var periods = new List<WorkingPeriod>
        {
            new(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(14, 0)),
            new(
                DayOfWeek.Monday,
                new TimeOnly(13, 0),
                new TimeOnly(18, 0))
        };

        // Act
        var action = () => restaurant.UpdateWorkingPeriods(periods);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void UpdateWorkingPeriods_WithAdjacentPeriods_DoesNotThrow()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Test Restaurant");

        var periods = new List<WorkingPeriod>
        {
            new(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(14, 0)),
            new(
                DayOfWeek.Monday,
                new TimeOnly(14, 0),
                new TimeOnly(18, 0))
        };

        // Act
        var action = () => restaurant.UpdateWorkingPeriods(periods);

        // Assert
        var exception = Record.Exception(action);

        Assert.Null(exception);
        Assert.Equal(2, restaurant.WorkingPeriods.Count);
    }

    [Fact]
    public void UpdateWorkingPeriods_WhenInvalidScheduleIsProvided_PreservesPreviousSchedule()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Test Restaurant");

        var originalPeriod = new WorkingPeriod(
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(14, 0));

        restaurant.UpdateWorkingPeriods([originalPeriod]);

        var invalidPeriods = new List<WorkingPeriod>
        {
            new(
                DayOfWeek.Monday,
                new TimeOnly(10, 0),
                new TimeOnly(15, 0)),
            new(
                DayOfWeek.Monday,
                new TimeOnly(13, 0),
                new TimeOnly(18, 0))
        };

        // Act
        var action = () =>
            restaurant.UpdateWorkingPeriods(invalidPeriods);

        // Assert
        Assert.Throws<DomainException>(action);

        Assert.Single(restaurant.WorkingPeriods);
        Assert.Equal(originalPeriod, restaurant.WorkingPeriods.Single());
    }

    [Fact]
    public void UpdateWorkingPeriods_WithEmptyCollection_ClearsSchedule()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Test Restaurant");

        restaurant.UpdateWorkingPeriods(
        [
            new WorkingPeriod(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(14, 0))
        ]);

        // Act
        restaurant.UpdateWorkingPeriods([]);

        // Assert
        Assert.Empty(restaurant.WorkingPeriods);
    }

    [Fact]
    public void UpdateWorkingPeriods_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        var restaurant = new RestaurantEntity("Test Restaurant");

        // Act
        var action = () => restaurant.UpdateWorkingPeriods(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }
}
