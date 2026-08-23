using DashboardHost.Models;

namespace DashboardHost.Services;

/// <summary>
/// Holds the single, live copy of <see cref="DashboardSettings"/> shared between the
/// settings page (writer) and <see cref="MetricsBroadcastService"/> (reader). Registered
/// as a singleton; access is synchronized with a lock since the background service reads
/// it from a different execution context than the Blazor circuit that writes it.
/// </summary>
public class DashboardSettingsService
{
    private readonly object _gate = new();
    private DashboardSettings _current = new();

    public DashboardSettings Current
    {
        get
        {
            lock (_gate)
            {
                // Return a copy so callers can't mutate shared state without going through Update.
                return new DashboardSettings { RefreshIntervalSeconds = _current.RefreshIntervalSeconds };
            }
        }
    }

    public void Update(DashboardSettings settings)
    {
        lock (_gate)
        {
            _current = new DashboardSettings { RefreshIntervalSeconds = settings.RefreshIntervalSeconds };
        }
    }
}
