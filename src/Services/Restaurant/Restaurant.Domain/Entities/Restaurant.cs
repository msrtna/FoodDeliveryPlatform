using Restaurant.Domain.Common;
using Restaurant.Domain.Exceptions;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Entities;

public sealed class Restaurant : BaseEntity
{
    private readonly List<WorkingPeriod> _workingPeriods = [];

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public RestaurantAddress Address { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<WorkingPeriod> WorkingPeriods =>
        _workingPeriods.AsReadOnly();

    private Restaurant()
    {
    }

    public Restaurant(
        string name,
        RestaurantAddress address,
        string? description = null)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        Address = address ?? throw new ArgumentNullException(nameof(address));

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

    public void UpdateAddress(RestaurantAddress address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public void UpdateWorkingPeriods(
        IEnumerable<WorkingPeriod> workingPeriods)
    {
        ArgumentNullException.ThrowIfNull(workingPeriods);

        var periods = workingPeriods.ToList();

        if (periods.Any(period => period is null))
        {
            throw new DomainException(
                "Working periods cannot contain null values.");
        }

        foreach (var dayGroup in periods.GroupBy(
                     period => period.DayOfWeek))
        {
            var orderedPeriods = dayGroup
                .OrderBy(period => period.OpenTime)
                .ToList();

            for (var i = 1; i < orderedPeriods.Count; i++)
            {
                if (orderedPeriods[i].OpenTime <
                    orderedPeriods[i - 1].CloseTime)
                {
                    throw new DomainException(
                        $"Working periods cannot overlap on {dayGroup.Key}.");
                }
            }
        }

        _workingPeriods.Clear();
        _workingPeriods.AddRange(periods);
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