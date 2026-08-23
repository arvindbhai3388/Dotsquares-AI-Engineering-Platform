namespace TaskTracker.Api.Models;

/// <summary>
/// Lifecycle status of a <see cref="TaskItem"/>.
/// </summary>
/// <remarks>
/// Named <c>TaskItemStatus</c> rather than <c>TaskStatus</c> to avoid colliding with
/// <see cref="System.Threading.Tasks.TaskStatus"/>, which is in scope via ASP.NET Core's
/// implicit global usings.
/// </remarks>
public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}
