namespace ClientReportingPortal.Web.Contracts.Tasks;

/// <summary>
/// Response shape for the <c>/api/tasks</c> surface. Kept flat and JSON-friendly on purpose -
/// this is exactly the schema a Power Apps custom connector would import from the OpenAPI
/// document and turn into a data type inside a canvas app.
/// </summary>
public sealed class TaskItemDto
{
    public int Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public DateTimeOffset? DueDateUtc { get; init; }

    public string? AssignedTo { get; init; }
}
