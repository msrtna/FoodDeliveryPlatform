using MediatR;
using Restaurant.Application.Abstractions.Persistence;
using Restaurant.Application.DTOs.MenuCategories;

namespace Restaurant.Application.Features.MenuCategories.Queries;

public sealed class GetMenuCategoryByIdQueryHandler
    : IRequestHandler<GetMenuCategoryByIdQuery, MenuCategoryDto>
{
    private readonly IMenuCategoryRepository _menuCategoryRepository;

    public GetMenuCategoryByIdQueryHandler(
        IMenuCategoryRepository menuCategoryRepository)
    {
        _menuCategoryRepository = menuCategoryRepository;
    }

    public async Task<MenuCategoryDto> Handle(
        GetMenuCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _menuCategoryRepository.GetByIdAsync(
            request.RestaurantId,
            request.CategoryId,
            cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Menu category with ID '{request.CategoryId}' was not found.");
        }

        return new MenuCategoryDto(
            category.Id,
            category.RestaurantId,
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive);
    }
}
