using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public class WorkTask : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? MemberId { get; set; }
    public string? Category { get; set; }
}
