using Identity.Application.Abstractions.Authentication;
using Identity.Application.Abstractions.Identity;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Features.Authentication.Common;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Authentication.Refresh;

public sealed class RefreshHandler
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtOptions _jwtOptions;

    public RefreshHandler(
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
        RefreshCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokenHash = _refreshTokenService.HashToken(command.RefreshToken);

        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (existingToken is null)
        {
            return null;
        }

        if (existingToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeFamilyAsync(
                existingToken.FamilyId,
                cancellationToken);

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return null;
        }

        if (existingToken.IsExpired)
        {
            return null;
        }

        var user = await _identityService.GetByIdAsync(
            existingToken.UserId,
            cancellationToken);

        if (user is null)
        {
            await _refreshTokenRepository.RevokeFamilyAsync(
                existingToken.FamilyId,
                cancellationToken);

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            return null;
        }

        var newRefreshToken = _refreshTokenService.GenerateToken();
        var newRefreshTokenHash = _refreshTokenService.HashToken(newRefreshToken);

        var newRefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(
            _jwtOptions.RefreshTokenExpirationDays);

        var newRefreshTokenEntity = new RefreshToken(
            user.Id,
            newRefreshTokenHash,
            existingToken.FamilyId,
            newRefreshTokenExpiresAtUtc);

        existingToken.Revoke(newRefreshTokenEntity.Id);

        await _refreshTokenRepository.UpdateAsync(
            existingToken,
            cancellationToken);

        await _refreshTokenRepository.AddAsync(
            newRefreshTokenEntity,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        return new TokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshToken,
            newRefreshTokenExpiresAtUtc);
    }
}