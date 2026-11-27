using MediatR;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Commands;

public sealed record UpdateMenuItemCommand(
    Guid RestaurantId,
    Guid MenuCategoryId,
    Guid MenuItemId,
    string Name,
    decimal Price,
    string? Description,
    int DisplayOrder)
    : IRequest<MenuItemDto>;