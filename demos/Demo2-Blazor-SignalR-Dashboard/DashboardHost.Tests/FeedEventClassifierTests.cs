using DashboardHost.Services;
using SharedComponents.Models;

namespace DashboardHost.Tests;

public class FeedEventClassifierTests
{
    [Fact]
    public void BuildFeedEvent_ErrorRateAtOrAboveThreshold_ReturnsCriticalEvent()
    {
        var snapshot = new MetricSnapshot(DateTime.UtcNow, CpuPercent: 10, ActiveUsers: 5, ErrorRatePercent: 8);

        var feedEvent = FeedEventClassifier.BuildFeedEvent(snapshot, infoEventRoll: 0.99);

        Assert.NotNull(feedEvent);
        Assert.Equal(FeedEventLevel.Critical, feedEvent!.Level);
        Assert.Contains("Elevated error rate", feedEvent.Message);
    }

    [Fact]
    public void BuildFeedEvent_ErrorRateTakesPrecedenceOverCpuSpike()
    {
        // Both thresholds are breached at once; error rate (Critical) must win.
        var snapshot = new MetricSnapshot(DateTime.UtcNow, CpuPercent: 95, ActiveUsers: 5, ErrorRatePercent: 12);

        var feedEvent = FeedEventClassifier.BuildFeedEvent(snapshot, infoEventRoll: 0.99);

        Assert.NotNull(feedEvent);
        Assert.Equal(FeedEventLevel.Critical, feedEvent!.Level);
    }

    [Fact]
    public void BuildFeedEvent_CpuAtOrAboveThreshold_ReturnsWarningEvent()
    {
        var snapshot = new MetricSnapshot(DateTime.UtcNow, CpuPercent: 90, ActiveUsers: 5, ErrorRatePercent: 1);

        var feedEvent = FeedEventClassifier.BuildFeedEvent(snapshot, infoEventRoll: 0.99);

        Assert.NotNull(feedEvent);
        Assert.Equal(FeedEventLevel.Warning, feedEvent!.Level);
        Assert.Contains("CPU spike", feedEvent.Message);
    }

    [Fact]
    public void BuildFeedEvent_NothingElevated_RollBelowThreshold_ReturnsInfoEvent()
    {
        var snapshot = new MetricSnapshot(DateTime.UtcNow, CpuPercent: 40, ActiveUsers: 17, ErrorRatePercent: 1);

        var feedEvent = FeedEventClassifier.BuildFeedEvent(snapshot, infoEventRoll: 0.0);

        Assert.NotNull(feedEvent);
        Assert.Equal(FeedEventLevel.Info, feedEvent!.Level);
        Assert.Contains("Active users: 17", feedEvent.Message);
    }

    [Fact]
    public void BuildFeedEvent_NothingElevated_RollAtOrAboveThreshold_ReturnsNull()
    {
        var snapshot = new MetricSnapshot(DateTime.UtcNow, CpuPercent: 40, ActiveUsers: 17, ErrorRatePercent: 1);

        var feedEvent = FeedEventClassifier.BuildFeedEvent(snapshot, infoEventRoll: FeedEventClassifier.InfoEventProbability);

        Assert.Null(feedEvent);
    }

    [Theory]
    [InlineData(7.99)]
    [InlineData(0)]
    public void BuildFeedEvent_ErrorRateBelowThreshold_DoesNotReturnCritical(double errorRate)
    {
        var snapshot = new MetricSnapshot(DateTime.UtcNow, CpuPercent: 10, ActiveUsers: 5, ErrorRatePercent: errorRate);

        var feedEvent = FeedEventClassifier.BuildFeedEvent(snapshot, infoEventRoll: 0.99);

        Assert.True(feedEvent is null || feedEvent.Level != FeedEventLevel.Critical);
    }
}
