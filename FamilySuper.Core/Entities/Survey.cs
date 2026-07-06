namespace FamilySuper.Core.Entities;

public class Survey : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FieldsJson { get; set; } = "[]";
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
