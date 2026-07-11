namespace FamilySuper.Core.Entities;

/// <summary>
/// 用药计划实体
/// </summary>
public class MedicationPlan : EntityBase
{
    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 药物名称
    /// </summary>
    public string DrugName { get; set; } = string.Empty;

    /// <summary>
    /// 剂量
    /// </summary>
    public string? Dosage { get; set; }

    /// <summary>
    /// 服用频率
    /// </summary>
    public string? Frequency { get; set; }

    /// <summary>
    /// 每日次数
    /// </summary>
    public int TimesPerDay { get; set; } = 1;

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;
}
