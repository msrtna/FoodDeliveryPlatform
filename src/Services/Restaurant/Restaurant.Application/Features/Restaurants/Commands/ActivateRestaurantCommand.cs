using MediatR;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed record ActivateRestaurantCommand(Guid Id) : IRequest<bool>;