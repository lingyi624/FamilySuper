namespace FamilySuper.Core.Entities;

/// <summary>
/// 家庭媒体项实体
/// </summary>
public class MediaItem : EntityBase
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 媒体类型（照片/视频/音频/文档）
    /// </summary>
    public string MediaType { get; set; } = "照片";

    /// <summary>
    /// 分类（家庭合影/旅行/节日/日常/其他）
    /// </summary>
    public string Category { get; set; } = "其他";

    /// <summary>
    /// 标签（逗号分隔）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 拍摄/录制日期
    /// </summary>
    public DateTime? TakenDate { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否收藏
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }
}