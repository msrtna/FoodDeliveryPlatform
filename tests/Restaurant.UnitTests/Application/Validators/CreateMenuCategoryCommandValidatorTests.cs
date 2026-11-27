
using Restaurant.Application.Features.MenuCategories.Commands;
using Restaurant.Application.Features.MenuCategories.Validators;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class CreateMenuCategoryCommandValidatorTests
{
    private readonly CreateMenuCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new CreateMenuCategoryCommand(
            Guid.NewGuid(),
            "Main Dishes",
            "Main food menu",
            1);

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
            Name = new string('A', 101)
        };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Name");
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

    private static CreateMenuCategoryCommand CreateValidCommand()
    {
        return new CreateMenuCategoryCommand(
            Guid.NewGuid(),
            "Main Dishes",
            "Main food menu",
            1);
    }
}
