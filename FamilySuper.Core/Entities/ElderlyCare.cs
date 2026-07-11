namespace FamilySuper.Core.Entities;

/// <summary>
/// 紧急联络人实体
/// </summary>
public class EmergencyContact : EntityBase
{
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 与成员关系
    /// </summary>
    public string Relationship { get; set; } = string.Empty;

    /// <summary>
    /// 电话号码
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// 备用电话
    /// </summary>
    public string? AlternatePhone { get; set; }

    /// <summary>
    /// 是否为主要联络人
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 关联成员
    /// </summary>
    public FamilyMember? Member { get; set; }
}

/// <summary>
/// 健康指标实体
/// </summary>
public class HealthMetric : EntityBase
{
    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 关联成员
    /// </summary>
    public FamilyMember? Member { get; set; }

    /// <summary>
    /// 记录日期
    /// </summary>
    public DateTime? RecordDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 心率(bpm)
    /// </summary>
    public decimal? HeartRate { get; set; }

    /// <summary>
    /// 收缩压
    /// </summary>
    public decimal? SystolicPressure { get; set; }

    /// <summary>
    /// 舒张压
    /// </summary>
    public decimal? DiastolicPressure { get; set; }

    /// <summary>
    /// 血糖(mmol/L)
    /// </summary>
    public decimal? BloodSugar { get; set; }

    /// <summary>
    /// 体重(kg)
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// 体温(°C)
    /// </summary>
    public decimal? Temperature { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }
}