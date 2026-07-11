using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

/// <summary>
/// 工作任务实体
/// </summary>
public class WorkTask : EntityBase
{
    /// <summary>
    /// 任务标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 优先级（低/中/高/紧急）
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// 状态（待处理/进行中/已完成）
    /// </summary>
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;

    /// <summary>
    /// 截止日期
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string? Category { get; set; }
}
