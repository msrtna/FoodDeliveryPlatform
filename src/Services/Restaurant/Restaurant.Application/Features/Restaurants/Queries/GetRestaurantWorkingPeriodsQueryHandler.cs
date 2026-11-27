using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.Restaurants;

namespace Restaurant.Application.Features.Restaurants.Queries;

public sealed class GetRestaurantWorkingPeriodsQueryHandler
    : IRequestHandler<
        GetRestaurantWorkingPeriodsQuery,
        IReadOnlyCollection<WorkingPeriodDto>?>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public GetRestaurantWorkingPeriodsQueryHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<IReadOnlyCollection<WorkingPeriodDto>?> Handle(
        GetRestaurantWorkingPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.RestaurantId,
            cancellationToken);

        if (restaurant is null)
            return null;

        return restaurant.WorkingPeriods
            .OrderBy(period => period.DayOfWeek)
            .ThenBy(period => period.OpenTime)
            .Select(period => new WorkingPeriodDto(
                period.DayOfWeek,
                period.OpenTime,
                period.CloseTime))
            .ToList();
    }
}