
using FluentValidation;
using Restaurant.Application.Features.Restaurants.Commands;

namespace Restaurant.Application.Features.Restaurants.Validators;

public sealed class ActivateRestaurantCommandValidator
    : AbstractValidator<ActivateRestaurantCommand>
{
    public ActivateRestaurantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Restaurant ID is required.");
    }
}
