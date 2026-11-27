
using MediatR;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Queries;

public sealed record GetMenuItemByIdQuery(
    Guid RestaurantId,
    Guid MenuCategoryId,
    Guid MenuItemId)
    : IRequest<MenuItemDto>;
