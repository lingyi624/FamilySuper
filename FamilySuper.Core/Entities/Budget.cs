namespace FamilySuper.Core.Entities;

/// <summary>
/// 预算实体
/// </summary>
public class Budget : EntityBase
{
    /// <summary>
    /// 年份
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 月份
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 预算金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }
}
