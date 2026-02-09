namespace Quasar.Persistence.Relational.EfCore;

/// <summary>
/// Marker type identifying the framework-provided Notification read-model store.
/// Used with <see cref="ReadModelContext{TStore}"/> and <see cref="NotificationReadModelDefinition{TStore}"/>.
/// </summary>
public class NotificationStoreMarker : IReadModelStoreMarker { }
