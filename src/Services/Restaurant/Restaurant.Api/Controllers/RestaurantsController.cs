using MediatR;
using Microsoft.AspNetCore.Mvc;
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
}
