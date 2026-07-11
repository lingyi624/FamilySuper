using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

/// <summary>
/// 账单提醒实体
/// </summary>
public class BillReminder : EntityBase
{
    /// <summary>
    /// 账单名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 周期类型（月度/季度/年度）
    /// </summary>
    public BillCycleType CycleType { get; set; } = BillCycleType.Monthly;

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 上次支付日期
    /// </summary>
    public DateTime? LastPaidDate { get; set; }

    /// <summary>
    /// 下次到期日期
    /// </summary>
    public DateTime NextDueDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已支付
    /// </summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }
}
