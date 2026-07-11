using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

/// <summary>
/// 收支记录实体
/// </summary>
public class FinanceRecord : EntityBase
{
    /// <summary>
    /// 记录标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 类型（收入/支出）
    /// </summary>
    public FinanceType Type { get; set; } = FinanceType.Expense;

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; } = "其他";

    /// <summary>
    /// 记录日期
    /// </summary>
    public DateTime? RecordDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }
}
