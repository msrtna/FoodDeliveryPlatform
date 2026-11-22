using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Identity;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Features.Authentication.Common;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Authentication.Login;

public sealed class LoginHandler
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtOptions _jwtOptions;

    public LoginHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtOptions> jwtOptions)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<TokenResponse?> HandleAsync(
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

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        var refreshToken = _refreshTokenService.GenerateToken();
        var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);

        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(
            _jwtOptions.RefreshTokenExpirationDays);

        var refreshTokenEntity = new RefreshToken(
            user.Id,
            refreshTokenHash,
            Guid.CreateVersion7(),
            refreshTokenExpiresAtUtc);

        await _refreshTokenRepository.AddAsync(
            refreshTokenEntity,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        return new TokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc);
    }
}