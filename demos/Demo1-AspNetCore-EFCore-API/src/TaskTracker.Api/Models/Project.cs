using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Api.Models;

/// <summary>
/// A project that groups a set of related <see cref="TaskItem"/> records.
/// </summary>
public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
