using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public class BillReminder : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Category { get; set; }
    public BillCycleType CycleType { get; set; } = BillCycleType.Monthly;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastPaidDate { get; set; }
    public DateTime NextDueDate { get; set; } = DateTime.UtcNow;
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; } = true;
    public long? MemberId { get; set; }
    public string? Notes { get; set; }
}
