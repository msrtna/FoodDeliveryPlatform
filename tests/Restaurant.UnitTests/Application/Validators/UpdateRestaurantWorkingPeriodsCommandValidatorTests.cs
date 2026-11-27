
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Application.Features.Restaurants.Validators;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class UpdateRestaurantWorkingPeriodsCommandValidatorTests
{
    private readonly UpdateRestaurantWorkingPeriodsCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new UpdateRestaurantWorkingPeriodsCommand(
            Guid.NewGuid(),
            new[]
            {
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(18, 0))
            });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenRestaurantIdIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with
        {
            RestaurantId = Guid.Empty
        };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "RestaurantId");
    }

    [Fact]
    public void Validate_WhenWorkingPeriodsAreEmpty_ShouldHaveError()
    {
        var command = new UpdateRestaurantWorkingPeriodsCommand(
            Guid.NewGuid(),
            Array.Empty<WorkingPeriodInput>());

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "WorkingPeriods");
    }

    [Fact]
    public void Validate_WhenClosingTimeIsBeforeOpeningTime_ShouldHaveError()
    {
        var command = new UpdateRestaurantWorkingPeriodsCommand(
            Guid.NewGuid(),
            new[]
            {
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(18, 0),
                    new TimeOnly(9, 0))
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName.StartsWith("WorkingPeriods"));
    }

    [Fact]
    public void Validate_WhenClosingTimeEqualsOpeningTime_ShouldHaveError()
    {
        var command = new UpdateRestaurantWorkingPeriodsCommand(
            Guid.NewGuid(),
            new[]
            {
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(9, 0))
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName.StartsWith("WorkingPeriods"));
    }

    [Fact]
    public void Validate_WhenDayOfWeekIsInvalid_ShouldHaveError()
    {
        var command = new UpdateRestaurantWorkingPeriodsCommand(
            Guid.NewGuid(),
            new[]
            {
                new WorkingPeriodInput(
                    (DayOfWeek)10,
                    new TimeOnly(9, 0),
                    new TimeOnly(18, 0))
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName.StartsWith("WorkingPeriods"));
    }

    [Fact]
    public void Validate_WhenDayIsDuplicated_ShouldHaveError()
    {
        var command = new UpdateRestaurantWorkingPeriodsCommand(
            Guid.NewGuid(),
            new[]
            {
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(18, 0)),
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(10, 0),
                    new TimeOnly(19, 0))
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "WorkingPeriods");
    }

    private static UpdateRestaurantWorkingPeriodsCommand CreateValidCommand()
    {
        return new UpdateRestaurantWorkingPeriodsCommand(
            Guid.NewGuid(),
            new[]
            {
                new WorkingPeriodInput(
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(18, 0))
            });
    }
}
