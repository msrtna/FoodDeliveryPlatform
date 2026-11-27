
using FluentValidation;
using Restaurant.Application.Features.Restaurants.Commands;

namespace Restaurant.Application.Features.Restaurants.Validators;

public sealed class CreateRestaurantCommandValidator
    : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Restaurant name is required.")
            .MaximumLength(150)
            .WithMessage("Restaurant name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.Province)
            .NotEmpty()
            .WithMessage("Province is required.")
            .MaximumLength(100)
            .WithMessage("Province cannot exceed 100 characters.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.StreetAddress)
            .NotEmpty()
            .WithMessage("Street address is required.")
            .MaximumLength(500)
            .WithMessage("Street address cannot exceed 500 characters.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters.");
    }
}
