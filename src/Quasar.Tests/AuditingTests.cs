using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quasar.Auditing;
using Quasar.Cqrs;
using Quasar.Security;

namespace Quasar.Tests;

public class AuditingTests
{
    private sealed record AuditableCommand(Guid SubjectId, string Data) : ICommand<bool>, IAuthorizableRequest
    {
        public string Action => "test.auditable";
        public string Resource => "test";
    }

    private sealed class MockAuditStore : IAuditStore
    {
        public AuditEntry? LastEntry { get; private set; }
        public int CallCount { get; private set; }

        public Task StoreAsync(AuditEntry entry)
        {
            LastEntry = entry;
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class MockLogger<T> : ILogger<T>
    {
        public LogLevel? LastLevel { get; private set; }
        public string? LastMessage { get; private set; }

        public IDisposable BeginScope<TState>(TState state) => new MockScope();
        private sealed class MockScope : IDisposable { public void Dispose() { } }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LastLevel = logLevel;
            LastMessage = formatter(state, exception);
        }
    }

    [Fact]
    public async Task AuditBehavior_should_store_successful_command()
    {
        // Arrange
        var store = new MockAuditStore();
        var behavior = new AuditBehavior<AuditableCommand, bool>(store);
        var command = new AuditableCommand(Guid.NewGuid(), "test-data");
        var next = new Func<Task<bool>>(() => Task.FromResult(true));

        // Act
        await behavior.Handle(command, default, next);

        // Assert
        Assert.Equal(1, store.CallCount);
        Assert.NotNull(store.LastEntry);
        var entry = store.LastEntry!;
        Assert.True(entry.IsSuccess);
        Assert.Null(entry.Error);
        Assert.Equal(command.SubjectId, entry.SubjectId);
        Assert.Equal(command.GetType().FullName, entry.CommandType);
        Assert.Contains(command.Data, entry.CommandJson);
    }

    [Fact]
    public async Task AuditBehavior_should_store_failed_command()
    {
        // Arrange
        var store = new MockAuditStore();
        var behavior = new AuditBehavior<AuditableCommand, bool>(store);
        var command = new AuditableCommand(Guid.NewGuid(), "test-data");
        var exception = new InvalidOperationException("test-failure");
        var next = new Func<Task<bool>>(() => throw exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(command, default, next));

        // Assert
        Assert.Equal(1, store.CallCount);
        Assert.NotNull(store.LastEntry);
        var entry = store.LastEntry!;
        Assert.False(entry.IsSuccess);
        Assert.Equal(exception.Message, entry.Error);
        Assert.Equal(command.SubjectId, entry.SubjectId);
    }
    
    [Fact]
    public async Task AuditBehavior_should_not_audit_queries()
    {
        // Arrange
        var store = new MockAuditStore();
        var behavior = new AuditBehavior<IQuery<bool>, bool>(store);
        var query = new AuditableQuery();
        var next = new Func<Task<bool>>(() => Task.FromResult(true));

        // Act
        await behavior.Handle(query, default, next);

        // Assert
        Assert.Equal(0, store.CallCount);
    }
    
    private sealed record AuditableQuery : IQuery<bool>;

    [Fact]
    public async Task LoggingAuditStore_should_log_information_for_success()
    {
        // Arrange
        var logger = new MockLogger<LoggingAuditStore>();
        var store = new LoggingAuditStore(logger);
        var entry = new AuditEntry(DateTime.UtcNow, Guid.NewGuid(), "Test.Command", "{}", true, null);

        // Act
        await store.StoreAsync(entry);

        // Assert
        Assert.Equal(LogLevel.Information, logger.LastLevel);
        Assert.Contains("succeeded", logger.LastMessage);
    }

    [Fact]
    public async Task LoggingAuditStore_should_log_warning_for_failure()
    {
        // Arrange
        var logger = new MockLogger<LoggingAuditStore>();
        var store = new LoggingAuditStore(logger);
        var entry = new AuditEntry(DateTime.UtcNow, Guid.NewGuid(), "Test.Command", "{}", false, "Error");

        // Act
        await store.StoreAsync(entry);

        // Assert
        Assert.Equal(LogLevel.Warning, logger.LastLevel);
        Assert.Contains("failed", logger.LastMessage);
    }
}
