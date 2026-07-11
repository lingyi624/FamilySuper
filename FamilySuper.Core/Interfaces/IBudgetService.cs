using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 预算状态信息
/// </summary>
/// <param name="BudgetAmount">预算金额</param>
/// <param name="ActualExpense">实际支出</param>
/// <param name="Remaining">剩余金额</param>
/// <param name="UsagePercent">使用百分比</param>
public record BudgetStatus(decimal BudgetAmount, decimal ActualExpense, decimal Remaining, decimal UsagePercent);

/// <summary>
/// 预算服务接口
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// 获取预算列表
    /// </summary>
    /// <param name="year">年份筛选</param>
    /// <param name="month">月份筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>预算列表</returns>
    Task<List<Budget>> GetBudgetsAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或更新预算
    /// </summary>
    /// <param name="budget">预算</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>预算</returns>
    Task<Budget> AddOrUpdateAsync(Budget budget, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除预算
    /// </summary>
    /// <param name="id">预算ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取月度预算状态
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>预算及状态列表</returns>
    Task<List<(Budget Budget, BudgetStatus Status)>> GetMonthlyStatusAsync(int year, int month, CancellationToken cancellationToken = default);
}
