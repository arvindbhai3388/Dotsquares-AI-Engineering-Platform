using Microsoft.AspNetCore.SignalR;

namespace DashboardHost.Hubs;

/// <summary>
/// SignalR hub the dashboard page connects to. It has no client-invokable methods of
/// its own - metrics/events flow one way, server-to-client, pushed by
/// <see cref="DashboardHost.Services.MetricsBroadcastService"/> via
/// <c>IHubContext&lt;MetricsHub, IMetricsClient&gt;</c>. Declared as <c>Hub&lt;IMetricsClient&gt;</c>
/// (a strongly-typed hub) rather than the untyped <c>Hub</c>, per the framework's SignalR wiki
/// guidance, so a mismatched method name/payload is a compile error, not a silent runtime no-op.
/// Kept intentionally thin: a hub class is still required so SignalR has an endpoint/type
/// to map (<c>app.MapHub&lt;MetricsHub&gt;("/hubs/metrics")</c>) and to scope connected clients.
/// </summary>
public class MetricsHub : Hub<IMetricsClient>
{
}
