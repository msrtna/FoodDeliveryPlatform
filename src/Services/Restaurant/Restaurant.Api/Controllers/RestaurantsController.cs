using MediatR;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs.Restaurants;
using Restaurant.Application.Features.Restaurants.Commands;
using Restaurant.Application.Features.Restaurants.Queries;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/restaurants")]
public sealed class RestaurantsController : ControllerBase
{
    private readonly ISender _sender;

    public RestaurantsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRestaurantCommand command,
        CancellationToken cancellationToken)
    {
        var restaurantId = await _sender.Send(
            command,
            cancellationToken);

        return Created(
            $"/api/restaurants/{restaurantId}",
            new { id = restaurantId });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetRestaurantsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var restaurant = await _sender.Send(
            new GetRestaurantByIdQuery(id),
            cancellationToken);

        if (restaurant is null)
        {
            return NotFound();
        }

        return Ok(restaurant);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRestaurantCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var updated = await _sender.Send(
            command,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var activated = await _sender.Send(
            new ActivateRestaurantCommand(id),
            cancellationToken);

        if (!activated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deactivated = await _sender.Send(
            new DeactivateRestaurantCommand(id),
            cancellationToken);

        if (!deactivated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id:guid}/working-periods")]
    [ProducesResponseType(
    typeof(IReadOnlyCollection<WorkingPeriodDto>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingPeriods(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetRestaurantWorkingPeriodsQuery(id),
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id:guid}/working-periods")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkingPeriods(
    Guid id,
    [FromBody] IReadOnlyCollection<WorkingPeriodInput> workingPeriods,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateRestaurantWorkingPeriodsCommand(id, workingPeriods),
            cancellationToken);

        if (!result)
            return NotFound();

        return NoContent();
    }
}