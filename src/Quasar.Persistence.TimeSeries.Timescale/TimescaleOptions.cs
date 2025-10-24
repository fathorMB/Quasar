using System.ComponentModel.DataAnnotations;

namespace Quasar.Persistence.TimeSeries.Timescale;

public sealed class TimescaleOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    public string MetricsTable { get; set; } = "metrics";
    public string Schema { get; set; } = "public";
    public int WriteBatchSize { get; set; } = 500;
}
