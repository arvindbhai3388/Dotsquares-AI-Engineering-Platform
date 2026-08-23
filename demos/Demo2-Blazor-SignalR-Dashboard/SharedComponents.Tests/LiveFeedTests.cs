using Bunit;
using SharedComponents;
using SharedComponents.Models;
using Xunit;

namespace SharedComponents.Tests;

public class LiveFeedTests : TestContext
{
    [Fact]
    public void ShowsEmptyStateWhenNoEventsHaveBeenPushed()
    {
        var cut = RenderComponent<LiveFeed>();

        Assert.Contains("No events yet", cut.Markup);
    }

    [Fact]
    public void AddEventInsertsNewestEntryFirstAndUpdatesTheRenderedMarkup()
    {
        var cut = RenderComponent<LiveFeed>();

        cut.InvokeAsync(() => cut.Instance.AddEvent(new FeedEvent(DateTime.UtcNow, "First event", FeedEventLevel.Info)));
        cut.InvokeAsync(() => cut.Instance.AddEvent(new FeedEvent(DateTime.UtcNow, "Second event", FeedEventLevel.Warning)));

        Assert.Contains("Second event", cut.Markup);
        Assert.Contains("First event", cut.Markup);

        var items = cut.FindAll("li.live-feed__item");
        Assert.Equal(2, items.Count);
        Assert.Contains("Second event", items[0].TextContent);
    }

    [Fact]
    public void DropsOldestEventsBeyondMaxItems()
    {
        var cut = RenderComponent<LiveFeed>(parameters => parameters.Add(p => p.MaxItems, 2));

        for (var i = 1; i <= 3; i++)
        {
            var message = $"Event {i}";
            cut.InvokeAsync(() => cut.Instance.AddEvent(new FeedEvent(DateTime.UtcNow, message, FeedEventLevel.Info)));
        }

        Assert.Equal(2, cut.Instance.Events.Count);
        Assert.Contains("Event 3", cut.Markup);
        Assert.Contains("Event 2", cut.Markup);
        Assert.DoesNotContain("Event 1", cut.Markup);
    }
}
