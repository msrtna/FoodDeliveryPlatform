
using FluentValidation;
using MediatR;
using Restaurant.Application.Common.Behaviors;

namespace Restaurant.UnitTests.Application.Validators;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenRequestIsValid_ShouldExecuteNext()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Name)
            .NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, bool>(
            [validator]);

        var request = new TestRequest("Valid Name");
        var handlerExecuted = false;

        RequestHandlerDelegate<bool> next = _ =>
        {
            handlerExecuted = true;
            return Task.FromResult(true);
        };

        var result = await behavior.Handle(
            request,
            next,
            CancellationToken.None);

        Assert.True(result);
        Assert.True(handlerExecuted);
    }

    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ShouldThrowValidationException()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Name)
            .NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, bool>(
            [validator]);

        var request = new TestRequest("");
        var handlerExecuted = false;

        RequestHandlerDelegate<bool> next = _ =>
        {
            handlerExecuted = true;
            return Task.FromResult(true);
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(
                request,
                next,
                CancellationToken.None));

        Assert.False(handlerExecuted);
    }

    [Fact]
    public async Task Handle_WhenNoValidatorsExist_ShouldExecuteNext()
    {
        var behavior = new ValidationBehavior<TestRequest, bool>(
            []);

        var request = new TestRequest("Any Name");

        RequestHandlerDelegate<bool> next = _ =>
            Task.FromResult(true);

        var result = await behavior.Handle(
            request,
            next,
            CancellationToken.None);

        Assert.True(result);
    }

    private sealed record TestRequest(string Name);
}
