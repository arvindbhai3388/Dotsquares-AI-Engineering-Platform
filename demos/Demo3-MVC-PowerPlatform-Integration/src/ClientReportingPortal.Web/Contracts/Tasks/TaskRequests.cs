using System.ComponentModel.DataAnnotations;

namespace ClientReportingPortal.Web.Contracts.Tasks;

/// <summary>Request body for <c>POST /api/tasks</c>.</summary>
public sealed class CreateTaskRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Title { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public DateTimeOffset? DueDateUtc { get; init; }

    [StringLength(200)]
    public string? AssignedTo { get; init; }
}

/// <summary>Request body for <c>PUT /api/tasks/{id}</c>.</summary>
public sealed class UpdateTaskRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Title { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public DateTimeOffset? DueDateUtc { get; init; }

    [StringLength(200)]
    public string? AssignedTo { get; init; }
}
