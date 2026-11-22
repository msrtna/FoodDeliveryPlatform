using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Identity;

namespace Identity.Application.Features.Authentication.Login;

public sealed class LoginHandler
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AccessTokenResult?> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _identityService.AuthenticateAsync(
            command.Email,
            command.Password,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        return _jwtTokenService.GenerateAccessToken(user);
    }
}