namespace FamilySuper.Core.Entities;

/// <summary>
/// 照片时间墙实体
/// 用于按时间顺序展示家庭照片，记录照片的拍摄时间、地点、人物等信息
/// </summary>
public class PhotoTimeline : EntityBase
{
    /// <summary>
    /// 照片标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 照片文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 缩略图路径
    /// </summary>
    public string? ThumbnailPath { get; set; }

    /// <summary>
    /// 照片描述/备注
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 拍摄日期
    /// </summary>
    public DateTime? TakenDate { get; set; }

    /// <summary>
    /// 拍摄地点
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 纬度
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// 人物标签（逗号分隔）
    /// </summary>
    public string? PeopleTags { get; set; }

    /// <summary>
    /// 场景标签（逗号分隔）
    /// </summary>
    public string? SceneTags { get; set; }

    /// <summary>
    /// 照片分类（家庭/旅游/纪念日/生活/其他）
    /// </summary>
    public string Category { get; set; } = "生活";

    /// <summary>
    /// 关联的家庭成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 是否设为封面
    /// </summary>
    public bool IsCover { get; set; }

    /// <summary>
    /// 照片宽度（像素）
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// 照片高度（像素）
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long? FileSize { get; set; }
}
