using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DashboardHost.Tests;

/// <summary>
/// Confirms the metrics SignalR hub is actually mapped and reachable, mirroring Demo1's
/// TaskHub_NegotiateEndpoint_IsMapped pattern (see
/// TaskTracker.Tests/Integration/TaskTrackerApiIntegrationTests.cs).
/// </summary>
public class MetricsHubTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MetricsHubTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MetricsHub_NegotiateEndpoint_IsMapped()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/hubs/metrics/negotiate?negotiateVersion=1", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
