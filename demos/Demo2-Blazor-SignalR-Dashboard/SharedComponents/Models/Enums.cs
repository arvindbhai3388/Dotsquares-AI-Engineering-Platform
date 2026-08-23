namespace SharedComponents.Models;

/// <summary>
/// Direction a metric moved compared to its previous reading.
/// Drives the arrow/color shown by <see cref="global::SharedComponents.MetricCard"/>.
/// </summary>
public enum TrendDirection
{
    Flat,
    Up,
    Down
}

/// <summary>
/// Overall health of a monitored service, rendered by <see cref="global::SharedComponents.StatusBadge"/>.
/// </summary>
public enum ServiceStatus
{
    Healthy,
    Degraded,
    Down
}

/// <summary>
/// Severity of a single <see cref="FeedEvent"/> shown in the <see cref="global::SharedComponents.LiveFeed"/>.
/// </summary>
public enum FeedEventLevel
{
    Info,
    Warning,
    Critical
}
