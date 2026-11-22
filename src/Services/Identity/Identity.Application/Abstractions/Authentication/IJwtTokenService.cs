using Identity.Application.Abstractions.Identity;

namespace Identity.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(
        AuthenticatedUser user);
}