using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;
using Quasar.Security;
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

    private sealed record Add(int A, int B) : ICommand<int>;
    private sealed class AddHandler : ICommandHandler<Add, int>
    {
        public Task<int> Handle(Add command, CancellationToken cancellationToken = default)
            => Task.FromResult(command.A + command.B);
    }

    private sealed class ThrowingValidator : IValidator<Add>
    {
        public Task ValidateAsync(Add instance, CancellationToken cancellationToken = default)
        {
            if (instance.A < 0) throw new ValidationException("A must be >= 0");
            return Task.CompletedTask;
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
        services.AddScoped<ICommandHandler<Add, int>, AddHandler>();
        services.AddScoped<IValidator<Add>, ThrowingValidator>();
        var tx = new FlagTransaction();
        services.AddSingleton<ICommandTransaction>(tx);
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(new Add(-1, 2)));

        var ok = await mediator.Send(new Add(1, 2));
        Assert.Equal(3, ok);
        Assert.True(tx.Executed);
    }
}

