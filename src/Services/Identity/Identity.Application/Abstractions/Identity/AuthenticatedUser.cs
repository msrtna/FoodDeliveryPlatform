namespace Identity.Application.Abstractions.Identity;

public sealed record AuthenticatedUser(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyCollection<string> Roles);