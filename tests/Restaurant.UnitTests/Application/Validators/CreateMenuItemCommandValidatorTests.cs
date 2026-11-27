
using Restaurant.Application.Features.MenuItems.Commands;
using Restaurant.Application.Features.MenuItems.Validators;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class CreateMenuItemCommandValidatorTests
{
    private readonly CreateMenuItemCommandValidator _validator = new();

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
    public void Validate_WhenNameIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Name = "" };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenNameExceedsMaximumLength_ShouldHaveError()
    {
        var command = CreateValidCommand() with
        {
            Name = new string('A', 151)
        };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenPriceIsNegative_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Price = -1 };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Price");
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaximumLength_ShouldHaveError()
    {
        var command = CreateValidCommand() with
        {
            Description = new string('A', 501)
        };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WhenDisplayOrderIsNegative_ShouldHaveError()
    {
        var command = CreateValidCommand() with { DisplayOrder = -1 };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "DisplayOrder");
    }

    private static CreateMenuItemCommand CreateValidCommand()
    {
        return new CreateMenuItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Margherita Pizza",
            12.50m,
            "Classic Italian pizza",
            1);
    }
}
