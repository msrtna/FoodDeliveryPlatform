
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Application.Features.Restaurants.Validators;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class RestaurantActivationCommandValidatorTests
{
    [Fact]
    public void ActivateRestaurant_WhenIdIsValid_ShouldNotHaveErrors()
    {
        var validator = new ActivateRestaurantCommandValidator();
        var command = new ActivateRestaurantCommand(Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ActivateRestaurant_WhenIdIsEmpty_ShouldHaveError()
    {
        var validator = new ActivateRestaurantCommandValidator();
        var command = new ActivateRestaurantCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Id");
    }

    [Fact]
    public void DeactivateRestaurant_WhenIdIsValid_ShouldNotHaveErrors()
    {
        var validator = new DeactivateRestaurantCommandValidator();
        var command = new DeactivateRestaurantCommand(Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DeactivateRestaurant_WhenIdIsEmpty_ShouldHaveError()
    {
        var validator = new DeactivateRestaurantCommandValidator();
        var command = new DeactivateRestaurantCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Id");
    }
}
