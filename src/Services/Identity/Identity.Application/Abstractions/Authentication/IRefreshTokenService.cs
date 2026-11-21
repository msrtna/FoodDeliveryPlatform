namespace Identity.Application.Abstractions.Authentication;

public interface IRefreshTokenService
{
    string GenerateToken();

    string HashToken(string token);
}