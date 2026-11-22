using FluentValidation.TestHelper;
using Identity.Application.Features.Account.ChangeEmail;

namespace Identity.UnitTests.Features.Account.ChangeEmail;

public sealed class ChangeEmailCommandValidatorTests
{
    private readonly ChangeEmailCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new ChangeEmailCommand(
            "user-123",
            "new@example.com",
            "CurrentPassword123!");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var command = new ChangeEmailCommand(
            string.Empty,
            "new@example.com",
            "CurrentPassword123!");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveError()
    {
        var command = new ChangeEmailCommand(
            "user-123",
            string.Empty,
            "CurrentPassword123!");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewEmail);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var command = new ChangeEmailCommand(
            "user-123",
            "invalid-email",
            "CurrentPassword123!");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewEmail);
    }

    [Fact]
    public void Validate_WithEmailLongerThan256Characters_ShouldHaveError()
    {
        var email = new string('a', 250) + "@test.com";

        var command = new ChangeEmailCommand(
            "user-123",
            email,
            "CurrentPassword123!");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewEmail);
    }

    [Fact]
    public void Validate_WithEmptyCurrentPassword_ShouldHaveError()
    {
        var command = new ChangeEmailCommand(
            "user-123",
            "new@example.com",
            string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }
}