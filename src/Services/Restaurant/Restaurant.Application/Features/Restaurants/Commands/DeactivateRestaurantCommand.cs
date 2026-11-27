using MediatR;

namespace Restaurant.Application.Features.Restaurants.Commands;

public sealed record DeactivateRestaurantCommand(Guid Id) : IRequest<bool>;