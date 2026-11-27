
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Application.Features.Restaurants.Validators;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class CreateRestaurantCommandValidatorTests
{
    private readonly CreateRestaurantCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new CreateRestaurantCommand(
            "Pizza House",
            "Italian restaurant",
            "Tehran",
            "Tehran",
            "Valiasr Street",
            "1234567890");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveError()
    {
        var command = new CreateRestaurantCommand(
            "",
            "Italian restaurant",
            "Tehran",
            "Tehran",
            "Valiasr Street",
            "1234567890");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenNameExceedsMaximumLength_ShouldHaveError()
    {
        var command = new CreateRestaurantCommand(
            new string('A', 151),
            null,
            "Tehran",
            "Tehran",
            "Valiasr Street",
            null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenProvinceIsEmpty_ShouldHaveError()
    {
        var command = new CreateRestaurantCommand(
            "Pizza House",
            null,
            "",
            "Tehran",
            "Valiasr Street",
            null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Province");
    }

    [Fact]
    public void Validate_WhenCityIsEmpty_ShouldHaveError()
    {
        var command = new CreateRestaurantCommand(
            "Pizza House",
            null,
            "Tehran",
            "",
            "Valiasr Street",
            null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "City");
    }

    [Fact]
    public void Validate_WhenStreetAddressIsEmpty_ShouldHaveError()
    {
        var command = new CreateRestaurantCommand(
            "Pizza House",
            null,
            "Tehran",
            "Tehran",
            "",
            null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "StreetAddress");
    }
}
