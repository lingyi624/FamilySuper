namespace FamilySuper.Core.Entities;

/// <summary>
/// 家庭调研实体
/// </summary>
public class Survey : EntityBase
{
    /// <summary>
    /// 调研标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 调研描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 字段配置（JSON格式）
    /// </summary>
    public string FieldsJson { get; set; } = "[]";

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;
}
