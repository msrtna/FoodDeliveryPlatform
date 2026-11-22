using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefreshToken>> GetActiveByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _context.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                x.RevokedAtUtc == null &&
                x.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        await _context.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public Task UpdateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        _context.RefreshTokens.Update(refreshToken);

        return Task.CompletedTask;
    }

    public async Task RevokeFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken)
    {
        var tokens = await _context.RefreshTokens
            .Where(x =>
                x.FamilyId == familyId &&
                x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}