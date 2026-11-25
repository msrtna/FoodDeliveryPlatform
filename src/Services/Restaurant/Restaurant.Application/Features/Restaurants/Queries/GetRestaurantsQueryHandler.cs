using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.Common;
using Restaurant.Application.DTOs.Restaurants;

namespace Restaurant.Application.Features.Restaurants.Queries;

public sealed class GetRestaurantsQueryHandler
    : IRequestHandler<GetRestaurantsQuery, PagedResult<RestaurantDto>>
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private readonly IRestaurantRepository _restaurantRepository;

    public GetRestaurantsQueryHandler(
        IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<PagedResult<RestaurantDto>> Handle(
        GetRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(
            request.PageNumber,
            DefaultPageNumber);

        var pageSize = Math.Clamp(
            request.PageSize,
            DefaultPageSize,
            MaxPageSize);

        var (restaurants, totalCount) =
            await _restaurantRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                request.SearchTerm,
                request.IsActive,
                cancellationToken);

        var items = restaurants
            .Select(restaurant => new RestaurantDto(
                restaurant.Id,
                restaurant.Name,
                restaurant.Description,
                restaurant.Address.Province,
                restaurant.Address.City,
                restaurant.Address.StreetAddress,
                restaurant.Address.PostalCode,
                restaurant.IsActive,
                restaurant.CreatedAtUtc))
            .ToList();

        return new PagedResult<RestaurantDto>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }
}