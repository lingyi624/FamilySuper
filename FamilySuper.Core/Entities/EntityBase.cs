using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

/// <summary>
/// 实体基类，所有实体的公共基类
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// 主键标识
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否删除（软删除标记）
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 所属模式（大人/小孩）
    /// </summary>
    public string Mode { get; set; } = AppMode.Adult.ToString().ToLower();
}
