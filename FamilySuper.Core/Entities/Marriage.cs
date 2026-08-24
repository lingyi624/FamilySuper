using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

/// <summary>
/// 婚姻记录（支持多任配偶）
/// </summary>
public class Marriage : EntityBase
{
    /// <summary>
    /// 成员ID
    /// </summary>
    public long MemberId { get; set; }

    /// <summary>
    /// 配偶ID
    /// </summary>
    public long SpouseId { get; set; }

    /// <summary>
    /// 第几任（从1开始）
    /// </summary>
    public int MarriageOrder { get; set; } = 1;

    /// <summary>
    /// 备注（如：前妻/前夫）
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 成员（导航属性）
    /// </summary>
    public FamilyMember? Member { get; set; }

    /// <summary>
    /// 配偶（导航属性）
    /// </summary>
    public FamilyMember? Spouse { get; set; }
}
