namespace FamilySuper.Core.Entities;

/// <summary>
/// 会议记录实体
/// </summary>
public class MeetingNote : EntityBase
{
    /// <summary>
    /// 会议标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 会议日期
    /// </summary>
    public DateTime MeetingDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 会议时长（分钟）
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// 参会人员
    /// </summary>
    public string? Attendees { get; set; }

    /// <summary>
    /// 会议议程
    /// </summary>
    public string? Agenda { get; set; }

    /// <summary>
    /// 会议记录/笔记
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 行动项（JSON格式）
    /// </summary>
    public string? ActionItems { get; set; }

    /// <summary>
    /// 会议地点
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 状态（已安排/进行中/已结束/已取消）
    /// </summary>
    public string Status { get; set; } = "已安排";

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }
}