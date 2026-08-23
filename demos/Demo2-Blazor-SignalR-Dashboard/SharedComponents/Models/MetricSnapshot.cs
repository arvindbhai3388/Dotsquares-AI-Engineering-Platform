namespace SharedComponents.Models;

/// <summary>
/// A single point-in-time reading of the simulated system metrics.
/// Serialized as-is over the <c>MetricsHub</c> SignalR hub, so it is intentionally
/// a plain, dependency-free record that both a Blazor Server and a Blazor WebAssembly
/// client can deserialize identically.
/// </summary>
/// <param name="Timestamp">UTC time the reading was generated.</param>
/// <param name="CpuPercent">Simulated CPU utilization, 0-100.</param>
/// <param name="ActiveUsers">Simulated count of currently active users.</param>
/// <param name="ErrorRatePercent">Simulated request error rate, 0-100.</param>
public record MetricSnapshot(
    DateTime Timestamp,
    double CpuPercent,
    int ActiveUsers,
    double ErrorRatePercent);
