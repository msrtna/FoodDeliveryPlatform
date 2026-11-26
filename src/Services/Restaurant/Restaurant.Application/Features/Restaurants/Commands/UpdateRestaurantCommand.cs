using MediatR;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed record UpdateRestaurantCommand(
    Guid Id,
    string Name,
    string? Description,
    string Province,
    string City,
    string StreetAddress,
    string? PostalCode) : IRequest<bool>;