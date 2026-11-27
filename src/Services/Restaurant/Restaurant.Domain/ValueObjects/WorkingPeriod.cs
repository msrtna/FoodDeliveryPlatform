using Restaurant.Domain.Exceptions;

namespace Restaurant.Domain.ValueObjects;

public sealed record WorkingPeriod
{
    public DayOfWeek DayOfWeek { get; }

    public TimeOnly OpenTime { get; }

    public TimeOnly CloseTime { get; }

    public WorkingPeriod(
        DayOfWeek dayOfWeek,
        TimeOnly openTime,
        TimeOnly closeTime)
    {
        if (!Enum.IsDefined(dayOfWeek))
        {
            throw new DomainException("Invalid day of week.");
        }
        if (openTime >= closeTime)
        {
            throw new DomainException(
                "Opening time must be earlier than closing time.");
        }

        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
    }
}