namespace FamilySuper.Core.Entities;

/// <summary>
/// 家庭物品实体
/// </summary>
public class HomeItem : EntityBase
{
    /// <summary>
    /// 物品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分类（家具/家电/数码/衣物/书籍/其他）
    /// </summary>
    public string Category { get; set; } = "其他";

    /// <summary>
    /// 存放位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 购买日期
    /// </summary>
    public DateTime? PurchaseDate { get; set; }

    /// <summary>
    /// 购买价格
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// 保修到期日
    /// </summary>
    public DateTime? WarrantyExpiry { get; set; }

    /// <summary>
    /// 物品状态（正常/维修中/已报废）
    /// </summary>
    public string Status { get; set; } = "正常";

    /// <summary>
    /// 品牌/型号
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 关联成员ID
    /// </summary>
    public long? MemberId { get; set; }
}