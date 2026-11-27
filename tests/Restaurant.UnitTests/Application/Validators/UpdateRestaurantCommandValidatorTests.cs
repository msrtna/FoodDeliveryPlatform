
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Application.Features.Restaurants.Validators;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class UpdateRestaurantCommandValidatorTests
{
    private readonly UpdateRestaurantCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new UpdateRestaurantCommand(
            Guid.NewGuid(),
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
    public void Validate_WhenIdIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Id = Guid.Empty };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Id");
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Name = "" };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenProvinceIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Province = "" };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Province");
    }

    [Fact]
    public void Validate_WhenCityIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { City = "" };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "City");
    }

    [Fact]
    public void Validate_WhenStreetAddressIsEmpty_ShouldHaveError()
    {
        var command = CreateValidCommand() with { StreetAddress = "" };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "StreetAddress");
    }

    private static UpdateRestaurantCommand CreateValidCommand()
    {
        return new UpdateRestaurantCommand(
            Guid.NewGuid(),
            "Pizza House",
            "Italian restaurant",
            "Tehran",
            "Tehran",
            "Valiasr Street",
            "1234567890");
    }
}
