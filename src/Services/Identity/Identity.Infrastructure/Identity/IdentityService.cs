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
            return CreateFailureResult(result);
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

        return MapToAuthenticatedUser(user, roles);
    }

    public async Task<AuthenticatedUser?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return MapToAuthenticatedUser(user, roles);
    }

    public async Task<IdentityOperationResult> UpdateProfileAsync(
        string userId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            return IdentityOperationResult.Failure(
                new[]
                {
                    new IdentityOperationError(
                        "UserNotFound",
                        "The user was not found.")
                });
        }

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return CreateFailureResult(result);
        }

        return IdentityOperationResult.Success(user.Id);
    }

    private static IdentityOperationResult CreateFailureResult(
        IdentityResult result)
    {
        var errors = result.Errors
            .Select(error => new IdentityOperationError(
                error.Code,
                error.Description))
            .ToArray();

        return IdentityOperationResult.Failure(errors);
    }

    private static AuthenticatedUser MapToAuthenticatedUser(
        ApplicationUser user,
        IList<string> roles)
    {
        return new AuthenticatedUser(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email ?? string.Empty,
            roles.ToArray());
    }

    public async Task<IdentityOperationResult> ChangeEmailAsync(
    string userId,
    string newEmail,
    string currentPassword,
    CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            return IdentityOperationResult.Failure(
                new[]
                {
                new IdentityOperationError(
                    "UserNotFound",
                    "The user was not found.")
                });
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(
            user,
            currentPassword);

        if (!passwordIsValid)
        {
            return IdentityOperationResult.Failure(
                new[]
                {
                new IdentityOperationError(
                    "InvalidCurrentPassword",
                    "The current password is incorrect.")
                });
        }

        var normalizedEmail = newEmail.Trim();

        if (string.Equals(
            user.Email,
            normalizedEmail,
            StringComparison.OrdinalIgnoreCase))
        {
            return IdentityOperationResult.Success(user.Id);
        }

        var existingUser = await _userManager.FindByEmailAsync(
            normalizedEmail);

        if (existingUser is not null && existingUser.Id != user.Id)
        {
            return IdentityOperationResult.Failure(
                new[]
                {
                new IdentityOperationError(
                    "DuplicateEmail",
                    "This email address is already registered.")
                });
        }

        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return CreateFailureResult(result);
        }

        return IdentityOperationResult.Success(user.Id);
    }
}