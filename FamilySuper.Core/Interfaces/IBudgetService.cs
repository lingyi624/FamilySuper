using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public record BudgetStatus(decimal BudgetAmount, decimal ActualExpense, decimal Remaining, decimal UsagePercent);

public interface IBudgetService
{
    Task<List<Budget>> GetBudgetsAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default);
    Task<Budget> AddOrUpdateAsync(Budget budget, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<List<(Budget Budget, BudgetStatus Status)>> GetMonthlyStatusAsync(int year, int month, CancellationToken cancellationToken = default);
}
