using DashboardHost.Hubs;
using Microsoft.AspNetCore.SignalR;
using SharedComponents.Models;
using SharedComponents.Services;

namespace DashboardHost.Services;

/// <summary>
/// Background service that ticks on an interval (configurable from the Settings page via
/// <see cref="DashboardSettingsService"/>), generates the next simulated metric reading
/// through <see cref="IMetricsGenerator"/>, and broadcasts it to every connected dashboard
/// client over the <see cref="MetricsHub"/> SignalR hub. No external dependency or network
/// call is involved - everything is generated in-process.
/// </summary>
public class MetricsBroadcastService : BackgroundService
{
    private readonly IHubContext<MetricsHub, IMetricsClient> _hubContext;
    private readonly IMetricsGenerator _generator;
    private readonly DashboardSettingsService _settings;
    private readonly ILogger<MetricsBroadcastService> _logger;
    private readonly Random _random = new();

    public MetricsBroadcastService(
        IHubContext<MetricsHub, IMetricsClient> hubContext,
        IMetricsGenerator generator,
        DashboardSettingsService settings,
        ILogger<MetricsBroadcastService> logger)
    {
        _hubContext = hubContext;
        _generator = generator;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var snapshot = _generator.Next();
                await _hubContext.Clients.All.ReceiveMetric(snapshot, stoppingToken);

                var feedEvent = FeedEventClassifier.BuildFeedEvent(snapshot, _random.NextDouble());
                if (feedEvent is not null)
                {
                    await _hubContext.Clients.All.ReceiveEvent(feedEvent, stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to broadcast a metrics tick.");
            }

            var delaySeconds = Math.Clamp(_settings.Current.RefreshIntervalSeconds, 2, 60);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }
}
