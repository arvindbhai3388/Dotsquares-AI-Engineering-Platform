using SharedComponents.Models;

namespace DashboardHost.Services;

/// <summary>
/// Pure critical/warning/info threshold decision logic, extracted from
/// <see cref="MetricsBroadcastService"/> purely so it is independently unit-testable without
/// spinning up the background service (which requires a live <c>IHubContext</c> and a running
/// host). <see cref="MetricsBroadcastService"/> still owns the single <see cref="Random"/>
/// instance and the tick loop; it just delegates the "what event, if any, does this snapshot
/// produce" decision here. Behavior/output is unchanged from the original inline method.
/// </summary>
public static class FeedEventClassifier
{
    /// <summary>Probability, per tick, of surfacing a routine "active users" info event when
    /// nothing critical or warning-worthy is happening.</summary>
    public const double InfoEventProbability = 0.3;

    /// <summary>
    /// Decides which feed event (if any) a metric snapshot should raise.
    /// </summary>
    /// <param name="snapshot">The metric reading just generated.</param>
    /// <param name="infoEventRoll">
    /// The "should this tick also surface a routine info event" dice roll, in [0, 1).
    /// Passed in (rather than rolled internally) so this decision is deterministic and testable;
    /// callers should pass <c>Random.NextDouble()</c> to preserve the original random behavior.
    /// </param>
    public static FeedEvent? BuildFeedEvent(MetricSnapshot snapshot, double infoEventRoll)
    {
        if (snapshot.ErrorRatePercent >= 8)
        {
            return new FeedEvent(DateTime.UtcNow, $"Elevated error rate: {snapshot.ErrorRatePercent:0.00}%", FeedEventLevel.Critical);
        }

        if (snapshot.CpuPercent >= 90)
        {
            return new FeedEvent(DateTime.UtcNow, $"CPU spike: {snapshot.CpuPercent:0.0}%", FeedEventLevel.Warning);
        }

        // Occasionally surface a routine info event so the feed feels alive even when nothing is wrong.
        if (infoEventRoll < InfoEventProbability)
        {
            return new FeedEvent(DateTime.UtcNow, $"Active users: {snapshot.ActiveUsers}", FeedEventLevel.Info);
        }

        return null;
    }
}
