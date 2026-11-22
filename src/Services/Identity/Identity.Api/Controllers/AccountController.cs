using FluentValidation;
using Identity.Api.Models;
using Identity.Application.Abstractions.Identity;
using Identity.Application.Features.Account.ChangeEmail;
using Identity.Application.Features.Account.UpdateProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AccountController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly UpdateProfileHandler _updateProfileHandler;
    private readonly IValidator<UpdateProfileCommand> _updateProfileValidator;
    private readonly ChangeEmailHandler _changeEmailHandler;
    private readonly IValidator<ChangeEmailCommand> _changeEmailValidator;

    public AccountController(
        IIdentityService identityService,
        UpdateProfileHandler updateProfileHandler,
        IValidator<UpdateProfileCommand> updateProfileValidator,
        ChangeEmailHandler changeEmailHandler,
        IValidator<ChangeEmailCommand> changeEmailValidator)
    {
        _identityService = identityService;
        _updateProfileHandler = updateProfileHandler;
        _updateProfileValidator = updateProfileValidator;
        _changeEmailHandler = changeEmailHandler;
        _changeEmailValidator = changeEmailValidator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var user = await _identityService.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.Roles
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var command = new UpdateProfileCommand(
            userId,
            request.FirstName,
            request.LastName);

        var validationResult = await _updateProfileValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Errors = validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .ToArray()
            });
        }

        var result = await _updateProfileHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(error => error.Code == "UserNotFound"))
            {
                return NotFound(new { Errors = result.Errors });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }

    [HttpPut("email")]
    public async Task<IActionResult> ChangeEmail(
    [FromBody] ChangeEmailRequest request,
    CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var command = new ChangeEmailCommand(
            userId,
            request.NewEmail,
            request.CurrentPassword);

        var validationResult = await _changeEmailValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(
                error => error.ErrorMessage));
        }

        var result = await _changeEmailHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(error => error.Code == "UserNotFound"))
            {
                return NotFound();
            }

            if (result.Errors.Any(error => error.Code == "DuplicateEmail"))
            {
                return Conflict(result.Errors);
            }

            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}
