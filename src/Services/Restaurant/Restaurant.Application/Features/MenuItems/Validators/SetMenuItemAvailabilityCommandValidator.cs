
using FluentValidation;
using Restaurant.Application.Features.MenuItems.Commands;

namespace Restaurant.Application.Features.MenuItems.Validators;

public sealed class SetMenuItemAvailabilityCommandValidator
    : AbstractValidator<SetMenuItemAvailabilityCommand>
{
    public SetMenuItemAvailabilityCommandValidator()
    {
        RuleFor(x => x.RestaurantId)
            .NotEmpty()
            .WithMessage("Restaurant ID is required.");

        RuleFor(x => x.MenuCategoryId)
            .NotEmpty()
            .WithMessage("Menu category ID is required.");

        RuleFor(x => x.MenuItemId)
            .NotEmpty()
            .WithMessage("Menu item ID is required.");
    }
}
