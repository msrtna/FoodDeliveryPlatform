using MediatR;
using Restaurant.Application.DTOs.Restaurants;

namespace Restaurant.Application.Features.Restaurants.Queries;

public sealed record GetRestaurantByIdQuery(Guid Id)
    : IRequest<RestaurantDto?>;
