using MediatR;
using Restaurant.Application.DTOs.MenuCategories;

namespace Restaurant.Application.Features.MenuCategories.Queries;

public sealed record GetMenuCategoryByIdQuery(
    Guid RestaurantId,
    Guid CategoryId)
    : IRequest<MenuCategoryDto>;
