namespace FamilySuper.Core.Entities;

/// <summary>
/// 购物项实体
/// </summary>
public class ShoppingItem : EntityBase
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分类
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 预估价格
    /// </summary>
    public decimal? EstimatedPrice { get; set; }

    /// <summary>
    /// 状态（待购买/已购买/已取消）
    /// </summary>
    public ShoppingStatus Status { get; set; } = ShoppingStatus.Pending;

    /// <summary>
    /// 购买日期
    /// </summary>
    public DateTime? PurchasedDate { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }

    /// <summary>
    /// 关联成员（导航属性）
    /// </summary>
    public FamilyMember? Member { get; set; }
}

/// <summary>
/// 购物状态枚举
/// </summary>
public enum ShoppingStatus
{
    /// <summary>
    /// 待购买
    /// </summary>
    Pending,

    /// <summary>
    /// 已购买
    /// </summary>
    Purchased,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled
}
