namespace FamilySuper.Core.Entities;

/// <summary>
/// 学习计划实体
/// </summary>
public class StudyPlan : EntityBase
{
    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 科目/学科
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 学习目标
    /// </summary>
    public string? Goal { get; set; }

    /// <summary>
    /// 每周目标小时数
    /// </summary>
    public int TargetHoursPerWeek { get; set; } = 5;

    /// <summary>
    /// 进度百分比
    /// </summary>
    public int ProgressPercent { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }
}
