using Microsoft.Extensions.DependencyInjection;
using Quasar.Core;
using Quasar.Cqrs;
using Quasar.Security;
using System.Collections.Generic;
using Xunit;

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
}
