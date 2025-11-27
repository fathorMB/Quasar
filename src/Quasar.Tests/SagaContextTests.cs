using System;
using System.Threading;
using System.Threading.Tasks;
using Quasar.Cqrs;
using Quasar.Sagas;
using Xunit;

namespace Quasar.Tests;

public class SagaContextTests
{
    [Fact]
    public void SagaExecutionResult_Ignore_sets_status_correctly()
    {
        var result = SagaExecutionResult.Ignore();
        Assert.Equal(SagaExecutionStatus.Ignored, result.Status);
        // Assert.True(result.IsCompleted); // Check implementation
    }

    [Fact]
    public void SagaExecutionResult_Status_setter_works()
    {
        var initial = SagaExecutionResult.Ignore();
        var result = initial with { Status = SagaExecutionStatus.Completed };
        Assert.Equal(SagaExecutionStatus.Completed, result.Status);
    }

    [Fact]
    public void SagaContext_properties_are_set_correctly()
    {
        var state = new TestState { Id = Guid.NewGuid() };
        var context = new SagaContext<TestState>(state, null!, null!);

        Assert.Equal(state, context.State);
        Assert.Equal(state.Id, context.SagaId);
        Assert.Equal(CancellationToken.None, context.CancellationToken);
    }

    [Fact]
    public void GetService_returns_service_from_provider()
    {
        var state = new TestState();
        var service = new TestService();
        var provider = new FakeServiceProvider { Service = service };
        var context = new SagaContext<TestState>(state, provider, null!);

        var actual = context.GetService<TestService>();
        Assert.Same(service, actual);
    }

    [Fact]
    public async Task SendAsync_delegates_to_mediator()
    {
        var state = new TestState();
        var mediator = new FakeMediator();
        var context = new SagaContext<TestState>(state, null!, mediator);
        var command = new TestCommand();
        var token = new CancellationTokenSource().Token;

        await context.SendAsync(command, token);

        Assert.Same(command, mediator.LastCommand);
        Assert.Equal(token, mediator.LastToken);
    }

    [Fact]
    public async Task QueryAsync_delegates_to_mediator()
    {
        var state = new TestState();
        var mediator = new FakeMediator();
        var context = new SagaContext<TestState>(state, null!, mediator);
        var query = new TestQuery();
        var token = new CancellationTokenSource().Token;

        await context.QueryAsync(query, token);

        Assert.Same(query, mediator.LastCommand);
        Assert.Equal(token, mediator.LastToken);
    }

    public class TestState : SagaState { }
    public class TestService { }
    public class TestCommand : ICommand<bool> { }
    public class TestQuery : IQuery<bool> { }

    public class FakeServiceProvider : IServiceProvider
    {
        public object? Service { get; set; }
        public object? GetService(Type serviceType) => Service;
    }

    public class FakeMediator : IMediator
    {
        public object? LastCommand { get; private set; }
        public CancellationToken LastToken { get; private set; }

        public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            LastToken = cancellationToken;
            return Task.FromResult(default(TResult)!);
        }

        public Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            LastCommand = query;
            LastToken = cancellationToken;
            return Task.FromResult(default(TResult)!);
        }
    }
}
