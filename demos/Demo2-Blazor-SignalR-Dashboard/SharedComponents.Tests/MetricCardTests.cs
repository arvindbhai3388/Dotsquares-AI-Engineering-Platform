using Bunit;
using SharedComponents;
using SharedComponents.Models;
using Xunit;

namespace SharedComponents.Tests;

public class MetricCardTests : TestContext
{
    [Fact]
    public void RendersTitleValueAndUnit()
    {
        var cut = RenderComponent<MetricCard>(parameters => parameters
            .Add(p => p.Title, "CPU")
            .Add(p => p.Value, "42.3")
            .Add(p => p.Unit, "%"));

        Assert.Contains("CPU", cut.Markup);
        Assert.Contains("42.3", cut.Markup);
        Assert.Contains("%", cut.Markup);
    }

    [Theory]
    [InlineData(TrendDirection.Up, "metric-card--trend-up")]
    [InlineData(TrendDirection.Down, "metric-card--trend-down")]
    [InlineData(TrendDirection.Flat, "metric-card--trend-flat")]
    public void AppliesTrendCssClassForEachDirection(TrendDirection trend, string expectedClass)
    {
        var cut = RenderComponent<MetricCard>(parameters => parameters
            .Add(p => p.Title, "Active Users")
            .Add(p => p.Value, "120")
            .Add(p => p.Trend, trend));

        var root = cut.Find("div.metric-card");
        Assert.Contains(expectedClass, root.ClassList);
    }

    [Fact]
    public void ReRendersWhenParametersChange()
    {
        var cut = RenderComponent<MetricCard>(parameters => parameters
            .Add(p => p.Title, "Error Rate")
            .Add(p => p.Value, "0.40")
            .Add(p => p.Trend, TrendDirection.Flat));

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Value, "1.20")
            .Add(p => p.Trend, TrendDirection.Up));

        Assert.Contains("1.20", cut.Markup);
        Assert.Contains("metric-card--trend-up", cut.Find("div.metric-card").ClassList);
    }
}
