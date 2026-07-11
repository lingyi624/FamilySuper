using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

/// <summary>
/// 家庭成员实体
/// </summary>
public class FamilyMember : EntityBase
{
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 身份证号码
    /// </summary>
    public string? IdCardNumber { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// 头像路径
    /// </summary>
    public string? AvatarPath { get; set; }

    /// <summary>
    /// 角色（成年人/儿童）
    /// </summary>
    public string Role { get; set; } = "成年人";

    /// <summary>
    /// 偏好设置（JSON格式）
    /// </summary>
    public string? Preferences { get; set; }

    /// <summary>
    /// 父亲ID
    /// </summary>
    public long? FatherId { get; set; }

    /// <summary>
    /// 母亲ID
    /// </summary>
    public long? MotherId { get; set; }

    /// <summary>
    /// 父亲（导航属性）
    /// </summary>
    public FamilyMember? Father { get; set; }

    /// <summary>
    /// 母亲（导航属性）
    /// </summary>
    public FamilyMember? Mother { get; set; }

    /// <summary>
    /// 子女列表（导航属性）
    /// </summary>
    public ICollection<FamilyMember>? Children { get; set; }
}
