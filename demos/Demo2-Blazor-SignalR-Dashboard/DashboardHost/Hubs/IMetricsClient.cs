using SharedComponents.Models;

namespace DashboardHost.Hubs;

/// <summary>
/// Client-callable methods for <see cref="MetricsHub"/>. Declaring this interface and using
/// <c>Hub&lt;IMetricsClient&gt;</c>/<c>IHubContext&lt;MetricsHub, IMetricsClient&gt;</c> gives
/// compile-time checking of method name and payload shape on the server side, per the
/// framework's SignalR wiki guidance (avoid stringly-typed <c>Clients.All.SendAsync("Name", ...)</c>).
/// </summary>
public interface IMetricsClient
{
    Task ReceiveMetric(MetricSnapshot snapshot, CancellationToken cancellationToken = default);

    Task ReceiveEvent(FeedEvent feedEvent, CancellationToken cancellationToken = default);
}
