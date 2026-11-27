using MediatR;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Commands;

public sealed record SetMenuItemAvailabilityCommand(
    Guid RestaurantId,
    Guid MenuCategoryId,
    Guid MenuItemId,
    bool IsAvailable)
    : IRequest<MenuItemDto>;