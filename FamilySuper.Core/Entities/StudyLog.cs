namespace FamilySuper.Core.Entities;

public class StudyLog : EntityBase
{
    public long StudyPlanId { get; set; }
    public long? MemberId { get; set; }
    public DateTime StudyDate { get; set; } = DateTime.UtcNow;
    public int DurationMinutes { get; set; }
    public string? Content { get; set; }
    public string Completion { get; set; } = "completed";
    public string? Notes { get; set; }
}
