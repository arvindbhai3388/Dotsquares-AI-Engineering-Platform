using SharedComponents.Models;

namespace SharedComponents.Services;

/// <summary>
/// Generates a self-contained, random-but-bounded "walk" of CPU/active-users/error-rate
/// readings. No external dependency or call is involved - this is pure, deterministic-given-a-seed
/// simulation logic, which is what makes it worth extracting and unit testing on its own
/// (see <c>SharedComponents.Tests/MetricsGeneratorTests.cs</c>).
/// </summary>
public sealed class MetricsGenerator : IMetricsGenerator
{
    private const double MinCpu = 5;
    private const double MaxCpu = 98;
    private const int MinActiveUsers = 10;
    private const int MaxActiveUsers = 500;
    private const double MinErrorRate = 0;
    private const double MaxErrorRate = 12;

    private readonly Random _random;

    private double _cpuPercent = 32;
    private int _activeUsers = 120;
    private double _errorRatePercent = 0.4;

    public MetricsGenerator() : this(Random.Shared)
    {
    }

    /// <summary>Allows a seeded <see cref="Random"/> to be injected so tests are deterministic.</summary>
    public MetricsGenerator(Random random)
    {
        _random = random;
    }

    public MetricSnapshot Next()
    {
        _cpuPercent = Clamp(_cpuPercent + NextDelta(6), MinCpu, MaxCpu);
        _activeUsers = (int)Clamp(_activeUsers + NextDelta(10), MinActiveUsers, MaxActiveUsers);
        _errorRatePercent = Clamp(_errorRatePercent + NextDelta(0.35), MinErrorRate, MaxErrorRate);

        return new MetricSnapshot(
            DateTime.UtcNow,
            Math.Round(_cpuPercent, 1),
            _activeUsers,
            Math.Round(_errorRatePercent, 2));
    }

    private double NextDelta(double maxSwing) => (_random.NextDouble() * 2 - 1) * maxSwing;

    private static double Clamp(double value, double min, double max) => Math.Min(max, Math.Max(min, value));
}
