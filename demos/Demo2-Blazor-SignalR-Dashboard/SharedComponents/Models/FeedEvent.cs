namespace SharedComponents.Models;

/// <summary>
/// A single entry rendered by the <see cref="global::SharedComponents.LiveFeed"/> component.
/// </summary>
/// <param name="Timestamp">UTC time the event occurred.</param>
/// <param name="Message">Human-readable description of the event.</param>
/// <param name="Level">Severity used to style the entry.</param>
public record FeedEvent(
    DateTime Timestamp,
    string Message,
    FeedEventLevel Level = FeedEventLevel.Info);
