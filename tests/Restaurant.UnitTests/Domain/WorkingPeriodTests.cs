using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.UnitTests.Domain;

public sealed class WorkingPeriodTests
{
    [Fact]
    public void Constructor_WithValidTimes_CreatesWorkingPeriod()
    {
        // Arrange
        var openTime = new TimeOnly(9, 0);
        var closeTime = new TimeOnly(23, 0);

        // Act
        var period = new WorkingPeriod(
            DayOfWeek.Monday,
            openTime,
            closeTime);

        // Assert
        Assert.Equal(DayOfWeek.Monday, period.DayOfWeek);
        Assert.Equal(openTime, period.OpenTime);
        Assert.Equal(closeTime, period.CloseTime);
    }

    [Fact]
    public void Constructor_WhenOpeningTimeIsAfterClosingTime_ThrowsDomainException()
    {
        // Arrange
        var openTime = new TimeOnly(23, 0);
        var closeTime = new TimeOnly(9, 0);

        // Act
        var action = () => new WorkingPeriod(
            DayOfWeek.Monday,
            openTime,
            closeTime);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_WhenOpeningTimeEqualsClosingTime_ThrowsDomainException()
    {
        // Arrange
        var time = new TimeOnly(12, 0);

        // Act
        var action = () => new WorkingPeriod(
            DayOfWeek.Monday,
            time,
            time);

        // Assert
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_WithDifferentDays_CreatesDifferentWorkingPeriods()
    {
        // Arrange & Act
        var mondayPeriod = new WorkingPeriod(
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(23, 0));

        var tuesdayPeriod = new WorkingPeriod(
            DayOfWeek.Tuesday,
            new TimeOnly(10, 0),
            new TimeOnly(22, 0));

        // Assert
        Assert.NotEqual(mondayPeriod, tuesdayPeriod);
    }

    [Fact]
    public void Constructor_WhenDayOfWeekIsInvalid_ThrowsDomainException()
    {
        // Arrange
        var invalidDay = (DayOfWeek)7;

        // Act
        var action = () => new WorkingPeriod(
            invalidDay,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        // Assert
        Assert.Throws<DomainException>(action);
    }
}