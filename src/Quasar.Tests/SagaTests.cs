using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Quasar.Sagas.Persistence.Relational.EfCore;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Cqrs;
using Quasar.Sagas;
using Quasar.Sagas.Persistence;
using Quasar.Sagas.Registration;
using Quasar.Web;

namespace Quasar.Tests;

public sealed class SagaTests
{
    [Fact]
    public async Task Saga_pipeline_creates_state_on_start_message()
    {
        var services = CreateServiceCollection();
        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var repository = provider.GetRequiredService<ISagaRepository<TestSagaState>>();

        var sagaId = Guid.NewGuid();

        await mediator.Send(new StartSagaCommand(sagaId));

        var state = await repository.FindAsync(sagaId);
        Assert.NotNull(state);
        Assert.False(state!.IsCompleted);
        Assert.Equal(sagaId, state.Id);
    }

    [Fact]
    public async Task Saga_pipeline_removes_state_when_completed()
    {
        var services = CreateServiceCollection();
        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var repository = provider.GetRequiredService<ISagaRepository<TestSagaState>>();

        var sagaId = Guid.NewGuid();

        await mediator.Send(new StartSagaCommand(sagaId));
        await mediator.Send(new ProgressSagaCommand(sagaId, true));

        var state = await repository.FindAsync(sagaId);
        Assert.Null(state);
    }

    [Fact]
    public async Task Saga_state_persists_with_sqlite_repository()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = CreateServiceCollection(builder =>
            builder.UseSagaDbContext((_, options) => options.UseSqlite(connection)));

        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var repository = provider.GetRequiredService<ISagaRepository<TestSagaState>>();

        var sagaId = Guid.NewGuid();
        await mediator.Send(new StartSagaCommand(sagaId));

        var state = await repository.FindAsync(sagaId);
        Assert.NotNull(state);

        await mediator.Send(new ProgressSagaCommand(sagaId, true));
        var deleted = await repository.FindAsync(sagaId);
        Assert.Null(deleted);
    }

    private static IServiceCollection CreateServiceCollection(Action<ISagaPersistenceBuilder>? persistence = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddQuasarMediator(addDefaultBehaviors: false);
        services.AddScoped<Quasar.Security.IAuthorizationService>(_ => new AllowAllAuthorizationService());
        services.AddScoped<ICommandHandler<StartSagaCommand, bool>, StartSagaCommandHandler>();
        services.AddScoped<ICommandHandler<ProgressSagaCommand, bool>, ProgressSagaCommandHandler>();

        services.AddQuasarSagas(cfg =>
        {
            cfg.AddSaga<TestSaga, TestSagaState>(builder =>
            {
                builder.StartsWith<StartSagaCommand>(cmd => cmd.CorrelationId);
                builder.Handles<ProgressSagaCommand>(cmd => cmd.CorrelationId);
            });
        }, persistence);

        if (!services.Any(d => d.ServiceType == typeof(Quasar.Security.IAuthorizationService)))
        {
            throw new InvalidOperationException("Authorization service not registered.");
        }

        return services;
    }

    private sealed class StartSagaCommand : ICommand<bool>
    {
        public StartSagaCommand(Guid correlationId) => CorrelationId = correlationId;
        public Guid CorrelationId { get; }
    }

    private sealed class ProgressSagaCommand : ICommand<bool>
    {
        public ProgressSagaCommand(Guid correlationId, bool shouldComplete)
        {
            CorrelationId = correlationId;
            ShouldComplete = shouldComplete;
        }

        public Guid CorrelationId { get; }
        public bool ShouldComplete { get; }
    }

    private sealed class StartSagaCommandHandler : ICommandHandler<StartSagaCommand, bool>
    {
        public Task<bool> Handle(StartSagaCommand command, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class ProgressSagaCommandHandler : ICommandHandler<ProgressSagaCommand, bool>
    {
        public Task<bool> Handle(ProgressSagaCommand command, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class TestSaga :
        ISagaStartedBy<StartSagaCommand, TestSagaState>,
        ISagaHandles<ProgressSagaCommand, TestSagaState>
    {
        public Task<SagaExecutionResult> HandleAsync(SagaContext<TestSagaState> context, StartSagaCommand message, CancellationToken cancellationToken = default)
        {
            context.State.CorrelationId = message.CorrelationId;
            context.State.StepCount++;
            return Task.FromResult(SagaExecutionResult.Continue(context.State));
        }

        public Task<SagaExecutionResult> HandleAsync(SagaContext<TestSagaState> context, ProgressSagaCommand message, CancellationToken cancellationToken = default)
        {
            context.State.StepCount++;
            if (message.ShouldComplete)
            {
                return Task.FromResult(SagaExecutionResult.Completed(context.State));
            }

            return Task.FromResult(SagaExecutionResult.Continue(context.State));
        }
    }

    private sealed class TestSagaState : SagaState
    {
        public Guid CorrelationId { get; set; }
        public int StepCount { get; set; }
    }
    private sealed class AllowAllAuthorizationService : Quasar.Security.IAuthorizationService
    {
        public Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
