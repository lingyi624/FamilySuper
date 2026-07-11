namespace FamilySuper.Core.Entities;

/// <summary>
/// 家乡实景虚拟化实体
/// </summary>
public class VirtualScene : EntityBase
{
    /// <summary>
    /// 场景标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 地点名称
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// 纬度
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// 场景描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图片路径（JSON数组）
    /// </summary>
    public string? ImagePaths { get; set; }

    /// <summary>
    /// 场景类型（家乡/旅游/故居/地标/其他）
    /// </summary>
    public string SceneType { get; set; } = "家乡";

    /// <summary>
    /// 全景图路径
    /// </summary>
    public string? PanoramaPath { get; set; }

    /// <summary>
    /// 3D模型路径
    /// </summary>
    public string? ModelPath { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 照片数量
    /// </summary>
    public int? PhotoCount { get; set; }

    /// <summary>
    /// 照片目录路径
    /// </summary>
    public string? PhotoDirectory { get; set; }

    /// <summary>
    /// 重建状态（pending/completed/failed/processing）
    /// </summary>
    public string? ReconstructionStatus { get; set; }

    /// <summary>
    /// 重建时间
    /// </summary>
    public DateTime? ReconstructedAt { get; set; }

    /// <summary>
    /// 重建进度（0-100）
    /// </summary>
    public int? ReconstructionProgress { get; set; }
}