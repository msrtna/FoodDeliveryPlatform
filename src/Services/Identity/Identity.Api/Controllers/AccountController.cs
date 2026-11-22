
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AccountController : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub"),

            Email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email"),

            FirstName = User.FindFirstValue(ClaimTypes.GivenName)
                        ?? User.FindFirstValue("given_name"),

            LastName = User.FindFirstValue(ClaimTypes.Surname)
                       ?? User.FindFirstValue("family_name"),

            Roles = User.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .ToArray()
        });
    }
}
