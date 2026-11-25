using MediatR;
using Restaurant.Application.DTOs.Common;
using Restaurant.Application.DTOs.Restaurants;

namespace Restaurant.Application.Features.Restaurants.Queries;

public sealed record GetRestaurantsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null)
    : IRequest<PagedResult<RestaurantDto>>;