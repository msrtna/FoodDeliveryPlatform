
using Restaurant.Application.Features.MenuItems.Commands;
using Restaurant.Application.Features.MenuItems.Validators;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class SetMenuItemAvailabilityCommandValidatorTests
{
    private readonly SetMenuItemAvailabilityCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = CreateValidCommand();

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
    public void Validate_WhenMenuCategoryIdIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with
        {
            MenuCategoryId = Guid.Empty
        };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "MenuCategoryId");
    }

    [Fact]
    public void Validate_WhenMenuItemIdIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with
        {
            MenuItemId = Guid.Empty
        };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "MenuItemId");
    }

    private static SetMenuItemAvailabilityCommand CreateValidCommand()
    {
        return new SetMenuItemAvailabilityCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            true);
    }
}
