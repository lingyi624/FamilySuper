namespace FamilySuper.Core.Entities;

/// <summary>
/// 三维场景标注实体
/// 用于在3D场景中添加注释和标记点
/// </summary>
public class Annotation : EntityBase
{
    /// <summary>
    /// 关联场景ID
    /// </summary>
    public long SceneId { get; set; }

    /// <summary>
    /// 标注位置X坐标
    /// </summary>
    public double PositionX { get; set; }

    /// <summary>
    /// 标注位置Y坐标
    /// </summary>
    public double PositionY { get; set; }

    /// <summary>
    /// 标注位置Z坐标
    /// </summary>
    public double PositionZ { get; set; }

    /// <summary>
    /// 标注文字内容
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 标注关联图片URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 标注颜色（十六进制）
    /// </summary>
    public string Color { get; set; } = "#ff0000";

    /// <summary>
    /// 关联记忆描述文字
    /// </summary>
    public string? MemoryText { get; set; }

    /// <summary>
    /// 记忆日期
    /// </summary>
    public DateTime? MemoryDate { get; set; }

    /// <summary>
    /// 相关媒体文件路径
    /// </summary>
    public string? RelatedMediaPath { get; set; }
}
