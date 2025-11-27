using System;
using System.Threading.Tasks;
using Quasar.Sagas;
using Quasar.Sagas.Persistence;
using Xunit;

namespace Quasar.Tests;

public class SagaRepositoryTests
{
    [Fact]
    public async Task InMemorySagaRepository_SaveAsync_persists_state()
    {
        var repository = new InMemorySagaRepository<TestState>();
        var id = Guid.NewGuid();
        var state = new TestState { Id = id, Value = "test" };

        await repository.SaveAsync(state);

        var loaded = await repository.FindAsync(id);
        Assert.NotNull(loaded);
        Assert.Equal(id, loaded!.Id);
        Assert.Equal("test", loaded.Value);
    }

    [Fact]
    public async Task InMemorySagaRepository_SaveAsync_updates_existing_state()
    {
        var repository = new InMemorySagaRepository<TestState>();
        var id = Guid.NewGuid();
        var state = new TestState { Id = id, Value = "initial" };

        await repository.SaveAsync(state);
        
        state.Value = "updated";
        await repository.SaveAsync(state);

        var loaded = await repository.FindAsync(id);
        Assert.NotNull(loaded);
        Assert.Equal("updated", loaded!.Value);
    }

    [Fact]
    public async Task InMemorySagaRepository_DeleteAsync_removes_state()
    {
        var repository = new InMemorySagaRepository<TestState>();
        var id = Guid.NewGuid();
        var state = new TestState { Id = id, Value = "test" };

        await repository.SaveAsync(state);
        var loaded = await repository.FindAsync(id);
        Assert.NotNull(loaded);

        await repository.DeleteAsync(id);

        var deleted = await repository.FindAsync(id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task InMemorySagaRepository_FindAsync_returns_null_for_unknown_id()
    {
        var repository = new InMemorySagaRepository<TestState>();
        var loaded = await repository.FindAsync(Guid.NewGuid());
        Assert.Null(loaded);
    }

    public class TestState : SagaState 
    { 
        public string Value { get; set; } = "";
    }
}
