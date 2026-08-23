using DashboardHost.Models;
using DashboardHost.Services;

namespace DashboardHost.Tests;

public class DashboardSettingsServiceTests
{
    [Fact]
    public void Current_BeforeAnyUpdate_ReturnsDefaultSettings()
    {
        var sut = new DashboardSettingsService();

        var current = sut.Current;

        Assert.Equal(5, current.RefreshIntervalSeconds);
    }

    [Fact]
    public void Update_ThenCurrent_RoundTripsTheNewValue()
    {
        var sut = new DashboardSettingsService();

        sut.Update(new DashboardSettings { RefreshIntervalSeconds = 42 });
        var current = sut.Current;

        Assert.Equal(42, current.RefreshIntervalSeconds);
    }

    [Fact]
    public void Current_ReturnsACopy_MutatingItDoesNotAffectSharedState()
    {
        var sut = new DashboardSettingsService();
        sut.Update(new DashboardSettings { RefreshIntervalSeconds = 10 });

        var snapshot = sut.Current;
        snapshot.RefreshIntervalSeconds = 999;

        Assert.Equal(10, sut.Current.RefreshIntervalSeconds);
    }

    [Fact]
    public void ConcurrentReadsAndWrites_DoNotThrow()
    {
        var sut = new DashboardSettingsService();

        var exception = Record.Exception(() =>
        {
            Parallel.For(0, 200, i =>
            {
                if (i % 2 == 0)
                {
                    sut.Update(new DashboardSettings { RefreshIntervalSeconds = 2 + (i % 58) });
                }
                else
                {
                    _ = sut.Current.RefreshIntervalSeconds;
                }
            });
        });

        Assert.Null(exception);
        // Whatever the last write happened to be, the value must still be within the valid range.
        Assert.InRange(sut.Current.RefreshIntervalSeconds, 2, 59);
    }
}
