using Bunit;
using SharedComponents;
using SharedComponents.Models;
using Xunit;

namespace SharedComponents.Tests;

public class StatusBadgeTests : TestContext
{
    [Theory]
    [InlineData(ServiceStatus.Healthy, "status-badge--healthy")]
    [InlineData(ServiceStatus.Degraded, "status-badge--degraded")]
    [InlineData(ServiceStatus.Down, "status-badge--down")]
    public void RendersCssClassMatchingStatus(ServiceStatus status, string expectedClass)
    {
        var cut = RenderComponent<StatusBadge>(parameters => parameters.Add(p => p.Status, status));

        Assert.Contains(expectedClass, cut.Find("span.status-badge").ClassList);
        Assert.Contains(status.ToString(), cut.Markup);
    }

    [Fact]
    public void LabelOverridesDefaultStatusText()
    {
        var cut = RenderComponent<StatusBadge>(parameters => parameters
            .Add(p => p.Status, ServiceStatus.Healthy)
            .Add(p => p.Label, "Live"));

        Assert.Contains("Live", cut.Markup);
        Assert.DoesNotContain("Healthy", cut.Markup);
    }
}
