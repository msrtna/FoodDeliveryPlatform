using Identity.Application.Features.Authentication.Register;

namespace Identity.UnitTests.Features.Authentication.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldPass()
    {
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenFirstNameIsEmpty_ShouldFail()
    {
        var command = new RegisterCommand(
            "",
            "Moradi",
            "mohammad@example.com",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterCommand.FirstName));
    }

    [Fact]
    public void Validate_WhenLastNameIsEmpty_ShouldFail()
    {
        var command = new RegisterCommand(
            "Mohammad",
            "",
            "mohammad@example.com",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterCommand.LastName));
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ShouldFail()
    {
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "invalid-email",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterCommand.Email));
    }

    [Fact]
    public void Validate_WhenPasswordIsTooShort_ShouldFail()
    {
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Ab1!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Validate_WhenPasswordHasNoUppercaseLetter_ShouldFail()
    {
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Validate_WhenPasswordHasNoNumber_ShouldFail()
    {
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Password!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Validate_WhenPasswordHasNoSpecialCharacter_ShouldFail()
    {
        var command = new RegisterCommand(
            "Mohammad",
            "Moradi",
            "mohammad@example.com",
            "Password123");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterCommand.Password));
    }
}