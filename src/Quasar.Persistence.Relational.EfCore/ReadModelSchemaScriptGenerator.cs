using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Generates SQL create scripts for a specific read model context. Useful for offline migrations and DBA review.
/// </summary>
/// <typeparam name="TContext">Read model <see cref="DbContext"/>.</typeparam>
public interface IReadModelSchemaScriptGenerator<TContext> where TContext : ReadModelContext
{
    /// <summary>
    /// Builds a provider-specific CREATE script for the configured read model context.
    /// </summary>
    Task<string> GenerateCreateScriptAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the generated script to <paramref name="path"/> using UTF-8 encoding.
    /// </summary>
    Task WriteCreateScriptAsync(string path, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class ReadModelSchemaScriptGenerator<TContext> : IReadModelSchemaScriptGenerator<TContext>
    where TContext : ReadModelContext
{
    private readonly IDbContextFactory<TContext> _factory;

    public ReadModelSchemaScriptGenerator(IDbContextFactory<TContext> factory)
    {
        _factory = factory;
    }

    public async Task<string> GenerateCreateScriptAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return context.Database.GenerateCreateScript();
    }

    public async Task WriteCreateScriptAsync(string path, CancellationToken cancellationToken = default)
    {
        var script = await GenerateCreateScriptAsync(cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(path, script, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
    }
}
