using System.ComponentModel.DataAnnotations;

namespace Quasar.Persistence.TimeSeries.Timescale;

/// <summary>
/// Configuration options controlling TimescaleDB integrations.
/// </summary>
public sealed class TimescaleOptions
{
    /// <summary>
    /// Gets or sets the connection string used to connect to the Timescale/PostgreSQL server.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional override for the database name. When provided the initializer will ensure the database exists.
    /// </summary>
    public string? Database { get; set; }
        = null; // optional override for database creation

    /// <summary>
    /// Gets or sets the table name that stores metric data.
    /// </summary>
    public string MetricsTable { get; set; } = "metrics";

    /// <summary>
    /// Gets or sets the schema where the metrics table resides.
    /// </summary>
    public string Schema { get; set; } = "public";

    /// <summary>
    /// Gets or sets the number of points batched per COPY operation.
    /// </summary>
    public int WriteBatchSize { get; set; } = 500;
}
