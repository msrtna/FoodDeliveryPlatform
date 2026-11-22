using System.Security.Cryptography;
using System.Text;
using Identity.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.WebUtilities;

namespace Identity.Infrastructure.Identity;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return WebEncoders.Base64UrlEncode(randomBytes);
    }

    public string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(
                "Token cannot be empty.",
                nameof(token));
        }

        var tokenBytes = Encoding.UTF8.GetBytes(token);

        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}