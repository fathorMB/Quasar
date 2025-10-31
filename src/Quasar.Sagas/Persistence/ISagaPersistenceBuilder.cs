using Microsoft.Extensions.DependencyInjection;

namespace Quasar.Sagas.Persistence;

/// <summary>
/// Allows selecting the persistence provider for sagas.
/// </summary>
public interface ISagaPersistenceBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Registers the provided repository implementation for saga state.
    /// </summary>
    /// <param name="openGenericRepositoryType">Open generic repository type implementing <see cref="ISagaRepository{TState}"/>.</param>
    void UseRepository(Type openGenericRepositoryType);

}
