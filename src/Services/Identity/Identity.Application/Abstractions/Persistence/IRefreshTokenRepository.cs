using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RefreshToken>> GetActiveByUserIdAsync(
        string userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);

    Task RevokeFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}