namespace FamilySuper.Core.Entities;

/// <summary>
/// 调研响应实体
/// </summary>
public class SurveyResponse : EntityBase
{
    /// <summary>
    /// 关联调研ID
    /// </summary>
    public long SurveyId { get; set; }

    /// <summary>
    /// 提交成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 响应值（JSON格式）
    /// </summary>
    public string ValuesJson { get; set; } = "{}";

    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
