using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;

namespace Quasar.Sagas;

public sealed class SagaContext<TState> where TState : class, ISagaState, new()
{
    private readonly IServiceProvider _services;
    private readonly IMediator _mediator;

    internal SagaContext(TState state, IServiceProvider services, IMediator mediator, CancellationToken cancellationToken)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        _services = services;
        _mediator = mediator;
        CancellationToken = cancellationToken;
    }

    public SagaContext(TState state, IServiceProvider services, IMediator mediator)
        : this(state, services, mediator, CancellationToken.None)
    {
    }

    public TState State { get; }

    public Guid SagaId => State.Id;

    public CancellationToken CancellationToken { get; }

    public TService GetService<TService>() where TService : notnull => _services.GetRequiredService<TService>();

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken == default ? CancellationToken : cancellationToken);

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => _mediator.Send(query, cancellationToken == default ? CancellationToken : cancellationToken);
}
