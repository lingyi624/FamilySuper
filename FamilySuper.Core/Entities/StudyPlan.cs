namespace FamilySuper.Core.Entities;

public class StudyPlan : EntityBase
{
    public long? MemberId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public int TargetHoursPerWeek { get; set; } = 5;
    public int ProgressPercent { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
