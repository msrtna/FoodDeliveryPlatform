using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Restaurant.Api.Middleware;

namespace Restaurant.UnitTests.Api.Middleware;

public sealed class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenValidationFails_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange
        RequestDelegate next = _ =>
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    "Name",
                    "Name is required.")
            ]);

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(
            StatusCodes.Status400BadRequest,
            context.Response.StatusCode);

        context.Response.Body.Position = 0;

        var response = await JsonDocument.ParseAsync(
            context.Response.Body);

        Assert.Equal(
            "Validation Failed",
            response.RootElement.GetProperty("title").GetString());

        Assert.Equal(
            "Name is required.",
            response.RootElement
                .GetProperty("errors")
                .GetProperty("Name")[0]
                .GetString());
    }
}
