namespace TaskTracker.Api.Options;

/// <summary>
/// Configurable bounds for list-endpoint pagination, bound from the
/// "Pagination" section of configuration.
/// </summary>
public class PaginationOptions
{
    public const string SectionName = "Pagination";

    public int DefaultPageSize { get; set; } = 10;

    public int MaxPageSize { get; set; } = 100;
}
