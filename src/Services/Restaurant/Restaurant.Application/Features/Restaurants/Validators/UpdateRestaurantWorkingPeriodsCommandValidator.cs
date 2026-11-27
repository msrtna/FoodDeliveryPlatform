
using FluentValidation;
using Restaurant.Application.Features.Restaurants.Commands;

namespace Restaurant.Application.Features.Restaurants.Validators;

public sealed class UpdateRestaurantWorkingPeriodsCommandValidator
    : AbstractValidator<UpdateRestaurantWorkingPeriodsCommand>
{
    public UpdateRestaurantWorkingPeriodsCommandValidator()
    {
        RuleFor(x => x.RestaurantId)
            .NotEmpty()
            .WithMessage("Restaurant ID is required.");

        RuleFor(x => x.WorkingPeriods)
            .NotNull()
            .WithMessage("Working periods are required.")
            .NotEmpty()
            .WithMessage("At least one working period is required.");

        RuleForEach(x => x.WorkingPeriods)
            .ChildRules(period =>
            {
                period.RuleFor(x => x.DayOfWeek)
                    .IsInEnum()
                    .WithMessage("Invalid day of week.");

                period.RuleFor(x => x.CloseTime)
                    .GreaterThan(x => x.OpenTime)
                    .WithMessage("Closing time must be later than opening time.");
            })
            .When(x => x.WorkingPeriods is not null);

        RuleFor(x => x.WorkingPeriods)
            .Must(periods =>
                periods is null ||
                periods.Select(p => p.DayOfWeek).Distinct().Count() == periods.Count)
            .WithMessage("Each day of the week can only be specified once.");
    }
}
