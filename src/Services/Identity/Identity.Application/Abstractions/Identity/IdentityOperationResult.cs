namespace Identity.Application.Abstractions.Identity;

public sealed record IdentityOperationError(
    string Code,
    string Description);

public sealed record IdentityOperationResult(
    bool Succeeded,
    string? UserId,
    IReadOnlyCollection<IdentityOperationError> Errors)
{
    public static IdentityOperationResult Success(string userId)
    {
        return new IdentityOperationResult(
            true,
            userId,
            Array.Empty<IdentityOperationError>());
    }

    public static IdentityOperationResult Failure(
        IReadOnlyCollection<IdentityOperationError> errors)
    {
        return new IdentityOperationResult(
            false,
            null,
            errors);
    }
}