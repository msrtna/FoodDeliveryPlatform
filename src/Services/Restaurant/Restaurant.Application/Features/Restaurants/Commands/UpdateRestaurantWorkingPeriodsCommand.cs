using MediatR;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed record UpdateRestaurantWorkingPeriodsCommand(
    Guid RestaurantId,
    IReadOnlyCollection<WorkingPeriodInput> WorkingPeriods)
    : IRequest<bool>;

public sealed record WorkingPeriodInput(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime);