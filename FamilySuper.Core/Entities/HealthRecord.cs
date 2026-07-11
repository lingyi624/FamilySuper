using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

/// <summary>
/// 健康记录实体
/// </summary>
public class HealthRecord : EntityBase
{
    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 记录类型
    /// </summary>
    public HealthRecordType RecordType { get; set; } = HealthRecordType.Other;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 记录日期
    /// </summary>
    public DateTime? RecordDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 医生姓名
    /// </summary>
    public string? Doctor { get; set; }

    /// <summary>
    /// 医院名称
    /// </summary>
    public string? Hospital { get; set; }

    /// <summary>
    /// 附件路径
    /// </summary>
    public string? AttachmentPath { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}
