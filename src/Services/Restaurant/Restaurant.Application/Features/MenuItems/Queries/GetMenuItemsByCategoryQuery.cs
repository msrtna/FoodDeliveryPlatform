using MediatR;
using Restaurant.Application.DTOs.MenuItems;

namespace Restaurant.Application.Features.MenuItems.Queries;

public sealed record GetMenuItemsByCategoryQuery(
    Guid RestaurantId,
    Guid MenuCategoryId,
    bool? IsAvailable = null)
    : IRequest<IReadOnlyCollection<MenuItemDto>>;