namespace Identity.Application.Abstractions.Authentication;

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);