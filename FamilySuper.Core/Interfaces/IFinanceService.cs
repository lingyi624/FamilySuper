using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 财务服务接口
/// </summary>
public interface IFinanceService
{
    /// <summary>
    /// 获取收支记录列表
    /// </summary>
    /// <param name="type">类型筛选</param>
    /// <param name="category">分类筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>收支记录列表</returns>
    Task<List<FinanceRecord>> GetRecordsAsync(FinanceType? type = null, string? category = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取收支记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>收支记录</returns>
    Task<FinanceRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加收支记录
    /// </summary>
    /// <param name="record">收支记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的收支记录</returns>
    Task<FinanceRecord> AddAsync(FinanceRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新收支记录
    /// </summary>
    /// <param name="record">收支记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(FinanceRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除收支记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取财务汇总
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>财务汇总信息</returns>
    Task<FinanceSummary> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// 财务汇总信息
/// </summary>
/// <param name="TotalIncome">总收入</param>
/// <param name="TotalExpense">总支出</param>
/// <param name="Balance">余额</param>
/// <param name="RecordCount">记录数量</param>
public record FinanceSummary(decimal TotalIncome, decimal TotalExpense, decimal Balance, int RecordCount);
