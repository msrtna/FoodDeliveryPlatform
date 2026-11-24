using MediatR;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed record CreateRestaurantCommand(
    string Name,
    string? Description,
    string Province,
    string City,
    string StreetAddress,
    string? PostalCode) : IRequest<Guid>;
