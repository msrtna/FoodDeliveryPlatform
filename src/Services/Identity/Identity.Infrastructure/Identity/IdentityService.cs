using Identity.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityOperationResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedEmail = email.Trim();

        var user = new ApplicationUser
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(error => new IdentityOperationError(
                    error.Code,
                    error.Description))
                .ToArray();

            return IdentityOperationResult.Failure(errors);
        }

        return IdentityOperationResult.Success(user.Id);
    }

    public async Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email.Trim());

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(
            user,
            password);

        if (!passwordIsValid)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthenticatedUser(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            roles.ToArray());
    }
}