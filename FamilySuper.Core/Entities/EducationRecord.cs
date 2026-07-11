namespace FamilySuper.Core.Entities;

/// <summary>
/// 教育记录实体
/// </summary>
public class EducationRecord : EntityBase
{
    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 科目
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 问题
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// 答案
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// 记录日期
    /// </summary>
    public DateTime? RecordDate { get; set; } = DateTime.UtcNow;
}
