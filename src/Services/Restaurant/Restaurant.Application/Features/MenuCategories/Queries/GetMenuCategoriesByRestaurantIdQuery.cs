
using MediatR;
using Restaurant.Application.DTOs.MenuCategories;

namespace Restaurant.Application.Features.MenuCategories.Queries;

public sealed record GetMenuCategoriesByRestaurantIdQuery(
    Guid RestaurantId,
    bool? IsActive = null)
    : IRequest<IReadOnlyCollection<MenuCategoryDto>>;
