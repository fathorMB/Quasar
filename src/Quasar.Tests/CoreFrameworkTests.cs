using Microsoft.Extensions.Logging.Abstractions;
using Quasar.Core;
using Quasar.Cqrs;
using Quasar.Domain;
using Quasar.EventSourcing.Abstractions;
using Quasar.Security;

namespace Quasar.Tests;

public class CoreFrameworkTests
{
    [Fact]
    public void Result_tracks_success_and_failure_states()
    {
        var ok = Result.Success();
        Assert.True(ok.IsSuccess);
        Assert.False(ok.IsFailure);
        Assert.Null(ok.Error);

        var error = new Error("ERR", "Boom");
        var fail = Result.Failure(error);
        Assert.True(fail.IsFailure);
        Assert.False(fail.IsSuccess);
        Assert.Equal(error, fail.Error);
        Assert.Equal("ERR: Boom", error.ToString());
    }

    [Fact]
    public void Result_generic_preserves_value_and_error()
    {
        var success = Result<int>.Success(42);
        Assert.True(success.IsSuccess);
        Assert.Equal(42, success.Value);
        Assert.Null(success.Error);

        var err = new Error("NOPE", "Invalid");
        var failure = Result<int>.Failure(err);
        Assert.True(failure.IsFailure);
        Assert.Equal(err, failure.Error);
        Assert.Equal(default, failure.Value);
    }

    [Fact]
    public void SystemClock_returns_utc_time_close_to_now()
    {
        var clock = new SystemClock();
        var before = DateTime.UtcNow;
        var value = clock.UtcNow;
        var after = DateTime.UtcNow;

        Assert.InRange(value, before.AddMilliseconds(-5), after.AddMilliseconds(5));
    }

    [Fact]
    public void GuidIds_generates_unique_values()
    {
        var first = GuidIds.New();
        var second = GuidIds.New();

        Assert.NotEqual(Guid.Empty, first);
        Assert.NotEqual(Guid.Empty, second);
        Assert.NotEqual(first, second);
    }

    private sealed class SampleValueObject(int X, int Y) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return X;
            yield return Y;
        }
    }

    [Fact]
    public void ValueObject_implements_value_equality()
    {
        var a = new SampleValueObject(1, 2);
        var b = new SampleValueObject(1, 2);
        var c = new SampleValueObject(2, 3);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, c);
    }

    private sealed record SampleEvent(string Value) : IEvent;

    [Fact]
    public void EventEnvelope_exposes_payload_and_metadata()
    {
        var evt = new SampleEvent("test");
        var metadata = new Dictionary<string, string>
        {
            ["source"] = "unit",
            ["correlationId"] = Guid.NewGuid().ToString()
        };
        var envelope = new EventEnvelope(Guid.NewGuid(), 3, DateTime.UtcNow, evt, metadata);

        Assert.Equal(evt, envelope.Event);
        Assert.Equal(3, envelope.Version);
        Assert.Equal("unit", envelope.Metadata?["source"]);
    }

    [Fact]
    public async Task NoopCommandTransaction_runs_delegate_directly()
    {
        var tx = new NoopCommandTransaction();
        var executed = false;
        var result = await tx.ExecuteAsync(async _ =>
        {
            executed = true;
            await Task.Delay(1);
            return 99;
        });

        Assert.True(executed);
        Assert.Equal(99, result);
    }

    [Fact]
    public void AclEntry_records_subject_and_effect()
    {
        var subject = Guid.NewGuid();
        var entry = new AclEntry(subject, "counter:1", "counter.increment", AclEffect.Allow);

        Assert.Equal(subject, entry.SubjectId);
        Assert.Equal(AclEffect.Allow, entry.Effect);
        Assert.Equal("counter.increment", entry.Action);
    }

    private sealed class AllowAllAuthorizationService(bool allow) : IAuthorizationService
    {
        public Task<bool> AuthorizeAsync(Guid subjectId, string action, string resource, CancellationToken cancellationToken = default)
            => Task.FromResult(Allow);

        public bool Allow { get; } = allow;
    }

    private sealed record AuthorizedCommand(Guid SubjectId, string Action, string Resource) :
        ICommand<int>, IAuthorizableRequest;

    [Fact]
    public async Task AuthorizationBehavior_allows_flow_when_authorized()
    {
        var service = new AllowAllAuthorizationService(true);
        var behavior = new AuthorizationBehavior<AuthorizedCommand, int>(service, NullLogger<AuthorizationBehavior<AuthorizedCommand, int>>.Instance);
        var command = new AuthorizedCommand(Guid.NewGuid(), "test.action", "resource/1");

        var result = await behavior.Handle(command, CancellationToken.None, () => Task.FromResult(5));

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task AuthorizationBehavior_denies_when_service_returns_false()
    {
        var service = new AllowAllAuthorizationService(false);
        var behavior = new AuthorizationBehavior<AuthorizedCommand, int>(service, NullLogger<AuthorizationBehavior<AuthorizedCommand, int>>.Instance);
        var command = new AuthorizedCommand(Guid.NewGuid(), "test.action", "resource/1");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => behavior.Handle(command, CancellationToken.None, () => Task.FromResult(5)));
    }

    private sealed record SampleCommand(Guid SubjectId, string Action, string Resource) :
        ICommand<int>, IAuthorizableRequest;

    private sealed record SampleQuery(string Value) : IQuery<string>;

    private sealed class CapturingTransaction : ICommandTransaction
    {
        public bool WasCalled { get; private set; }
        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return action(cancellationToken);
        }
    }

    [Fact]
    public async Task TransactionBehavior_wraps_commands_with_transaction()
    {
        var tx = new CapturingTransaction();
        var behavior = new TransactionBehavior<SampleCommand, int>(tx, NullLogger<TransactionBehavior<SampleCommand, int>>.Instance);
        var command = new SampleCommand(Guid.NewGuid(), "test.action", "resource/1");

        var result = await behavior.Handle(command, CancellationToken.None, () => Task.FromResult(11));

        Assert.True(tx.WasCalled);
        Assert.Equal(11, result);
    }

    [Fact]
    public async Task TransactionBehavior_skips_when_no_transaction_or_not_command()
    {
        var tx = new CapturingTransaction();
        var behaviorWithNull = new TransactionBehavior<SampleCommand, int>(null, NullLogger<TransactionBehavior<SampleCommand, int>>.Instance);
        var command = new SampleCommand(Guid.NewGuid(), "test.action", "resource/1");
        var result = await behaviorWithNull.Handle(command, CancellationToken.None, () => Task.FromResult(7));

        Assert.Equal(7, result);
        Assert.False(tx.WasCalled);

        var queryBehavior = new TransactionBehavior<SampleQuery, string>(tx, NullLogger<TransactionBehavior<SampleQuery, string>>.Instance);
        var queryResult = await queryBehavior.Handle(new SampleQuery("value"), CancellationToken.None, () => Task.FromResult("value"));

        Assert.False(tx.WasCalled);
        Assert.Equal("value", queryResult);
    }
}
