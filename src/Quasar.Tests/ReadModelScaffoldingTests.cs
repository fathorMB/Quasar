using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quasar.Persistence.Relational.EfCore;
using Xunit;

namespace Quasar.Tests;

public class ReadModelScaffoldingTests
{
    private sealed class TestStore : IReadModelStoreMarker { }
    private sealed class OtherStore : IReadModelStoreMarker { }

    private sealed class TestReadModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestReadModelDefinition : ReadModelDefinition<TestStore>
    {
        public override void Configure(ModelBuilder builder)
        {
            builder.Entity<TestReadModel>(e =>
            {
                e.ToTable("FakeEntities");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired();
            });
        }
    }

    private sealed class OtherReadModelDefinition : ReadModelDefinition<OtherStore>
    {
        public override void Configure(ModelBuilder builder)
        {
            builder.Entity<TestReadModel>(e => e.ToTable("Other"));
        }
    }

    [Fact]
    public void Registers_definitions_only_for_matching_store()
    {
        var services = new ServiceCollection();

        services.AddReadModelDefinitionsFromAssembliesForStore<TestStore>(typeof(TestReadModelDefinition).Assembly);

        var provider = services.BuildServiceProvider();
        var defs = provider.GetServices<IReadModelDefinition>().ToList();

        Assert.Contains(defs, d => d.GetType() == typeof(TestReadModelDefinition));
        Assert.DoesNotContain(defs, d => d.GetType() == typeof(OtherReadModelDefinition));
    }

    [Fact]
    public void Does_not_duplicate_existing_definition_registration()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IReadModelDefinition, TestReadModelDefinition>();
        services.AddReadModelDefinitionsFromAssembliesForStore<TestStore>(typeof(TestReadModelDefinition).Assembly);

        var provider = services.BuildServiceProvider();
        var defs = provider.GetServices<IReadModelDefinition>().Where(d => d is TestReadModelDefinition).ToList();

        Assert.Single(defs);
    }

    [Fact]
    public async Task Schema_script_generator_emits_expected_table()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton<IReadModelModelSource, ReadModelModelSource>();
        services.AddReadModelDefinitionsFromAssembliesForStore<TestStore>(typeof(TestReadModelDefinition).Assembly);
        services.AddDbContextFactory<ReadModelContext<TestStore>>(options =>
        {
            options.UseSqlite(connection);
        });
        services.AddSingleton<IReadModelSchemaScriptGenerator<ReadModelContext<TestStore>>, ReadModelSchemaScriptGenerator<ReadModelContext<TestStore>>>();

        var provider = services.BuildServiceProvider();
        var generator = provider.GetRequiredService<IReadModelSchemaScriptGenerator<ReadModelContext<TestStore>>>();

        var script = await generator.GenerateCreateScriptAsync();

        Assert.Contains("CREATE TABLE", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FakeEntities", script);
        Assert.Contains("Id", script);
        Assert.Contains("Name", script);
    }
}
