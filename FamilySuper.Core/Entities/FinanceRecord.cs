using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public class FinanceRecord : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public FinanceType Type { get; set; } = FinanceType.Expense;
    public string Category { get; set; } = "其他";
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;
    public string? Remark { get; set; }
    public long? MemberId { get; set; }
}
