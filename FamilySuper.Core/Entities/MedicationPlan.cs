namespace FamilySuper.Core.Entities;

public class MedicationPlan : EntityBase
{
    public long? MemberId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int TimesPerDay { get; set; } = 1;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
