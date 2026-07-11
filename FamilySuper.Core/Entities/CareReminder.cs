namespace FamilySuper.Core.Entities;

/// <summary>
/// 关怀提醒实体
/// </summary>
public class CareReminder : EntityBase
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; } = "通用";

    /// <summary>
    /// 提醒类型
    /// </summary>
    public CareReminderType Type { get; set; } = CareReminderType.Custom;

    /// <summary>
    /// 内容详情
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 提醒日期
    /// </summary>
    public DateTime? RemindDate { get; set; }

    /// <summary>
    /// 是否重复提醒
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// 重复周期模式
    /// </summary>
    public string? RecurrencePattern { get; set; }

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// 关怀提醒类型枚举
/// </summary>
public enum CareReminderType
{
    /// <summary>
    /// 自定义
    /// </summary>
    Custom,

    /// <summary>
    /// 养老关怀
    /// </summary>
    ElderlyCare,

    /// <summary>
    /// 用药提醒
    /// </summary>
    Medication,

    /// <summary>
    /// 生日
    /// </summary>
    Birthday,

    /// <summary>
    /// 健康检查
    /// </summary>
    HealthCheck,

    /// <summary>
    /// 儿童关怀
    /// </summary>
    ChildCare,

    /// <summary>
    /// 纪念日
    /// </summary>
    Anniversary
}