using MediatR;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs.MenuCategories;
using Restaurant.Application.Features.MenuCategories.Commands;
using Restaurant.Application.Features.MenuCategories.Queries;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/menu-categories")]
public sealed class MenuCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public MenuCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MenuCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid restaurantId,
        [FromBody] CreateMenuCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateMenuCategoryCommand(
                restaurantId,
                request.Name,
                request.Description,
                request.DisplayOrder),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                restaurantId,
                categoryId = result.Id
            },
            result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<MenuCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRestaurantId(
        Guid restaurantId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMenuCategoriesByRestaurantIdQuery(
                restaurantId,
                isActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{categoryId:guid}")]
    [ProducesResponseType(typeof(MenuCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid restaurantId,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMenuCategoryByIdQuery(
                restaurantId,
                categoryId),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateMenuCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder = 0);
