namespace Identity.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<IdentityOperationResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthenticatedUser?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken);
}