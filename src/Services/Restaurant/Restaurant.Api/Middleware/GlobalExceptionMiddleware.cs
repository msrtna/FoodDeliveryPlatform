
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Domain.Exceptions;

namespace Restaurant.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();

            if (exception is ValidationException validationException)
            {
                _logger.LogWarning(
                    "Request validation failed: {Errors}",
                    validationException.Message);

                var errors = validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .Distinct()
                            .ToArray());

                context.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(
                    new ValidationProblemDetails(errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Failed",
                        Detail = "One or more validation errors occurred."
                    },
                    cancellationToken: context.RequestAborted);

                return;
            }

            _logger.LogError(
                exception,
                "An unhandled exception occurred while processing the request.");

            var (statusCode, title, detail) = exception switch
            {
                KeyNotFoundException =>
                    (
                        StatusCodes.Status404NotFound,
                        "Resource Not Found",
                        exception.Message
                    ),

                DomainException =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Business Rule Violation",
                        exception.Message
                    ),

                _ =>
                    (
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "An unexpected error occurred."
                    )
            };

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail
                },
                cancellationToken: context.RequestAborted);
        }
    }
}
