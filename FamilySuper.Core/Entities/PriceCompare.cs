namespace FamilySuper.Core.Entities;

/// <summary>
/// 比价记录实体
/// </summary>
public class PriceCompare : EntityBase
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 商品分类
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 商店/平台名称
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// 价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 商品链接
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 是否最低价
    /// </summary>
    public bool IsLowest { get; set; }

    /// <summary>
    /// 比价日期
    /// </summary>
    public DateTime CompareDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }
}