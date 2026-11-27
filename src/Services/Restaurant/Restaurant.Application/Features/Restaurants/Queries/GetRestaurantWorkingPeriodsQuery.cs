using MediatR;
using Restaurant.Application.DTOs.Restaurants;

namespace Restaurant.Application.Features.Restaurants.Queries;

public sealed record GetRestaurantWorkingPeriodsQuery(Guid RestaurantId)
    : IRequest<IReadOnlyCollection<WorkingPeriodDto>?>;