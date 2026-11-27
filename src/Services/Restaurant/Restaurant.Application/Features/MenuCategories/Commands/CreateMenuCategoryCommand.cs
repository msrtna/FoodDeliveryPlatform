using MediatR;
using Restaurant.Application.DTOs.MenuCategories;

namespace Restaurant.Application.Features.MenuCategories.Commands;

public sealed record CreateMenuCategoryCommand(
    Guid RestaurantId,
    string Name,
    string? Description,
    int DisplayOrder)
    : IRequest<MenuCategoryDto>;