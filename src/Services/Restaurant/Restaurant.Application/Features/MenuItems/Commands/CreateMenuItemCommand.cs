using MediatR;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Commands;

public sealed record CreateMenuItemCommand(
    Guid RestaurantId,
    Guid MenuCategoryId,
    string Name,
    decimal Price,
    string? Description,
    int DisplayOrder)
    : IRequest<MenuItemDto>;