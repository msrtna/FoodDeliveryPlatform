using Restaurant.Domain.Common;
using Restaurant.Domain.Exceptions;

namespace Restaurant.Domain.Entities;

public sealed class Restaurant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Restaurant()
    {
    }

    public Restaurant(string name, string? description = null)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);

        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateInfo(string name, string? description)
    {
        var validatedName = ValidateName(name);
        var validatedDescription = ValidateDescription(description);

        Name = validatedName;
        Description = validatedDescription;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Restaurant name is required.");
        }

        name = name.Trim();

        if (name.Length > 150)
        {
            throw new DomainException(
                "Restaurant name cannot exceed 150 characters.");
        }

        return name;
    }

    private static string? ValidateDescription(string? description)
    {
        description = description?.Trim();

        if (description?.Length > 1000)
        {
            throw new DomainException(
                "Restaurant description cannot exceed 1000 characters.");
        }

        return string.IsNullOrEmpty(description)
            ? null
            : description;
    }
}