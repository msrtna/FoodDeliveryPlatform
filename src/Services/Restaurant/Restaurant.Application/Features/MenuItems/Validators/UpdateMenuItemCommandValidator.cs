
using FluentValidation;
using Restaurant.Application.Features.MenuItems.Commands;

namespace Restaurant.Application.Features.MenuItems.Validators;

public sealed class UpdateMenuItemCommandValidator
    : AbstractValidator<UpdateMenuItemCommand>
{
    public UpdateMenuItemCommandValidator()
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

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Menu item name is required.")
            .MaximumLength(150)
            .WithMessage("Menu item name cannot exceed 150 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order cannot be negative.");
    }
}
