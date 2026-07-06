namespace FamilySuper.Core.Entities;

public class EducationRecord : EntityBase
{
    public long? MemberId { get; set; }
    public string? Subject { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;
}
