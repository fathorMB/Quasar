using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Quasar.Core;
using Quasar.Cqrs;
using Quasar.Security;

namespace Quasar.Tests;

public class MediatorTests
{
    private sealed record Ping(string Message) : IQuery<string>;
    private sealed class PingHandler : IQueryHandler<Ping, string>
    {
        public Task<string> Handle(Ping query, CancellationToken cancellationToken = default)
            => Task.FromResult($"pong:{query.Message}");
    }

    private sealed record Add(int A, int B) : ICommand<Result<int>>;
    private sealed class AddHandler : ICommandHandler<Add, Result<int>>
    {
        public Task<Result<int>> Handle(Add command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<int>.Success(command.A + command.B));
    }

    private sealed class NegativeValidator : IValidator<Add>
    {
        public Task<List<Error>> ValidateAsync(Add instance, CancellationToken cancellationToken = default)
        {
            var errors = new List<Error>();
            if (instance.A < 0)
            {
                errors.Add(new Error("test.validation", "A must be >= 0"));
            }
            return Task.FromResult(errors);
        }
    }

    private sealed class FlagTransaction : ICommandTransaction
    {
        public bool Executed { get; private set; }
        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        {
            Executed = true;
            return await action(cancellationToken);
        }
    }
    
    private class OrderTrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly List<string> _order;
        public OrderTrackingBehavior(List<string> order) => _order = order;
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
        {
            _order.Add($"Before-{GetType().Name.Split('`')[0]}");
            var response = await next();
            _order.Add($"After-{GetType().Name.Split('`')[0]}");
            return response;
        }
    }
    
    private sealed class Behavior1<TRequest, TResponse> : OrderTrackingBehavior<TRequest, TResponse> { public Behavior1(List<string> order) : base(order) { } }
    private sealed class Behavior2<TRequest, TResponse> : OrderTrackingBehavior<TRequest, TResponse> { public Behavior2(List<string> order) : base(order) { } }
    private sealed class ThrowingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next) 
            => throw new TestException();
    }
    private sealed class TestException : Exception { }

    [Fact]
    public async Task Mediator_runs_query_handler()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddLogging();
        services.AddScoped<IQueryHandler<Ping, string>, PingHandler>();
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        var result = await mediator.Send(new Ping("x"));
        Assert.Equal("pong:x", result);
    }

    [Fact]
    public async Task Mediator_applies_validation_and_transaction_for_command()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddLogging();
        services.AddScoped<ICommandHandler<Add, Result<int>>, AddHandler>();
        services.AddScoped<IValidator<Add>, NegativeValidator>();
        var tx = new FlagTransaction();
        services.AddSingleton<ICommandTransaction>(tx);
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        var failedResult = await mediator.Send(new Add(-1, 2));
        Assert.True(failedResult.IsFailure);
        Assert.Equal("Validation.Failed", failedResult.Error!.Value.Code);

        var okResult = await mediator.Send(new Add(1, 2));
        Assert.True(okResult.IsSuccess);
        Assert.Equal(3, okResult.Value);
        Assert.True(tx.Executed);
    }
    
    [Fact]
    public async Task Mediator_throws_when_no_handler_is_registered()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(new Add(1, 2)));
        Assert.Contains("Handler not registered", ex.Message);
    }

    [Fact]
    public async Task Mediator_executes_behaviors_in_correct_order()
    {
        var order = new List<string>();
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddLogging();
        services.AddSingleton(order); // Share the list
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior1<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behavior2<,>));
        services.AddScoped<IQueryHandler<Ping, string>, PingHandler>();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new Ping("test"));

        Assert.Equal(4, order.Count);
        Assert.Equal("Before-Behavior1", order[0]);
        Assert.Equal("Before-Behavior2", order[1]);
        Assert.Equal("After-Behavior2", order[2]);
        Assert.Equal("After-Behavior1", order[3]);
    }

    [Fact]
    public async Task Mediator_propagates_exceptions_from_behaviors()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddLogging();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ThrowingBehavior<,>));
        services.AddScoped<IQueryHandler<Ping, string>, PingHandler>();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var ex = await Assert.ThrowsAsync<TargetInvocationException>(() => mediator.Send(new Ping("test")));
        Assert.IsType<TestException>(ex.InnerException);
    }
}