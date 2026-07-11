namespace FamilySuper.Core.Entities;

/// <summary>
/// 就医指南实体
/// </summary>
public class MedicalGuide : EntityBase
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 医院名称
    /// </summary>
    public string HospitalName { get; set; } = string.Empty;

    /// <summary>
    /// 科室
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// 医生姓名
    /// </summary>
    public string? DoctorName { get; set; }

    /// <summary>
    /// 医院地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 评分（1-5）
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// 分类（综合医院/专科医院/社区诊所/药房）
    /// </summary>
    public string Category { get; set; } = "综合医院";

    /// <summary>
    /// 就诊流程描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 挂号方式
    /// </summary>
    public string? RegistrationMethod { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }
}