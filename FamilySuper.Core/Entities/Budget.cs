namespace FamilySuper.Core.Entities;

public class Budget : EntityBase
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public long? MemberId { get; set; }
    public string? Notes { get; set; }
}
