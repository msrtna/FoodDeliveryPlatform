using FluentValidation;
using Identity.Application.Features.Authentication.Login;
using Identity.Application.Features.Authentication.Register;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly RegisterHandler _registerHandler;
    private readonly LoginHandler _loginHandler;
    private readonly IValidator<RegisterCommand> _registerValidator;
    private readonly IValidator<LoginCommand> _loginValidator;

    public AuthenticationController(
        RegisterHandler registerHandler,
        LoginHandler loginHandler,
        IValidator<RegisterCommand> registerValidator,
        IValidator<LoginCommand> loginValidator)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Errors = validationResult.Errors.Select(error => new
                {
                    error.PropertyName,
                    error.ErrorMessage
                })
            });
        }

        var result = await _registerHandler.HandleAsync(
            command,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                Errors = result.Errors
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            UserId = result.UserId,
            Message = "Registration completed successfully."
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Errors = validationResult.Errors.Select(error => new
                {
                    error.PropertyName,
                    error.ErrorMessage
                })
            });
        }

        var result = await _loginHandler.HandleAsync(
            command,
            cancellationToken);

        if (result is null)
        {
            return Unauthorized(new
            {
                Message = "Invalid email or password."
            });
        }

        return Ok(result);
    }
}