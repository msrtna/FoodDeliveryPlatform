
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs.MenuItems;
using Restaurant.Application.Features.MenuItems.Commands;
using Restaurant.Application.Features.MenuItems.Queries;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/menu-categories/{categoryId:guid}/menu-items")]
public sealed class MenuItemsController : ControllerBase
{
    private readonly ISender _sender;

    public MenuItemsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid restaurantId,
        Guid categoryId,
        [FromBody] CreateMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateMenuItemCommand(
                restaurantId,
                categoryId,
                request.Name,
                request.Price,
                request.Description,
                request.DisplayOrder),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                restaurantId,
                categoryId,
                menuItemId = result.Id
            },
            result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<MenuItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCategory(
        Guid restaurantId,
        Guid categoryId,
        [FromQuery] bool? isAvailable,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMenuItemsByCategoryQuery(
                restaurantId,
                categoryId,
                isAvailable),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{menuItemId:guid}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid restaurantId,
        Guid categoryId,
        Guid menuItemId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMenuItemByIdQuery(
                restaurantId,
                categoryId,
                menuItemId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{menuItemId:guid}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        Guid restaurantId,
        Guid categoryId,
        Guid menuItemId,
        [FromBody] UpdateMenuItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateMenuItemCommand(
                restaurantId,
                categoryId,
                menuItemId,
                request.Name,
                request.Price,
                request.Description,
                request.DisplayOrder),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{menuItemId:guid}/availability")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(
        Guid restaurantId,
        Guid categoryId,
        Guid menuItemId,
        [FromBody] SetMenuItemAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SetMenuItemAvailabilityCommand(
                restaurantId,
                categoryId,
                menuItemId,
                request.IsAvailable),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateMenuItemRequest(
    string Name,
    decimal Price,
    string? Description,
    int DisplayOrder = 0);

public sealed record UpdateMenuItemRequest(
    string Name,
    decimal Price,
    string? Description,
    int DisplayOrder);

public sealed record SetMenuItemAvailabilityRequest(
    bool IsAvailable);
