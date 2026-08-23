using SharedComponents.Models;

namespace SharedComponents.Services;

/// <summary>
/// Produces the next simulated <see cref="MetricSnapshot"/> in a series.
/// Kept as a plain, testable interface (no ASP.NET Core dependency) so the
/// generation algorithm can be unit tested in isolation from SignalR/hosting.
/// </summary>
public interface IMetricsGenerator
{
    /// <summary>Generates the next reading, evolving from whatever the generator last produced.</summary>
    MetricSnapshot Next();
}
