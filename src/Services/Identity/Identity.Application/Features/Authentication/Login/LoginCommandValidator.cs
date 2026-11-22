using FluentValidation;

namespace Identity.Application.Features.Authentication.Login;

public sealed class LoginCommandValidator
    : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("A valid email address is required.")
            .MaximumLength(256)
            .WithMessage("Email cannot exceed 256 characters.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}