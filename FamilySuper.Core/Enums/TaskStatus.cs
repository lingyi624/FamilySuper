namespace FamilySuper.Core.Enums;

/// <summary>
/// 任务状态枚举
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    Pending,

    /// <summary>
    /// 进行中
    /// </summary>
    InProgress,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 已逾期
    /// </summary>
    Overdue
}
