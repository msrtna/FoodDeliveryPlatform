using Identity.Application.Features.Authentication.Login;

namespace Identity.UnitTests.Features.Authentication.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldPass()
    {
        var command = new LoginCommand(
            "mohammad@example.com",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldFail()
    {
        var command = new LoginCommand(
            "",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ShouldFail()
    {
        var command = new LoginCommand(
            "invalid-email",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldFail()
    {
        var command = new LoginCommand(
            "mohammad@example.com",
            "");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(LoginCommand.Password));
    }
}