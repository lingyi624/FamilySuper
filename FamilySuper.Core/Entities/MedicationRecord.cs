namespace FamilySuper.Core.Entities;

public class MedicationRecord : EntityBase
{
    public long MedicationPlanId { get; set; }
    public long? MemberId { get; set; }
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "taken";
    public string? Note { get; set; }
}
