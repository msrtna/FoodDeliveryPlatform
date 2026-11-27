namespace Restaurant.Application.DTOs.Restaurants;

public sealed record WorkingPeriodDto(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime);