using MediatR;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Features.Restaurants.Commands;

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
}
