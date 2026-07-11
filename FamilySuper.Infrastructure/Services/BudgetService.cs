using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly IRepository<Budget> _budgetRepo;
    private readonly IRepository<FinanceRecord> _financeRepo;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(
        IRepository<Budget> budgetRepo,
        IRepository<FinanceRecord> financeRepo,
        ILogger<BudgetService> logger)
    {
        _budgetRepo = budgetRepo;
        _financeRepo = financeRepo;
        _logger = logger;
    }

    public async Task<List<Budget>> GetBudgetsAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default)
    {
        var budgets = await _budgetRepo.GetAllAsync(cancellationToken);
        if (year is not null) budgets = budgets.Where(b => b.Year == year).ToList();
        if (month is not null) budgets = budgets.Where(b => b.Month == month).ToList();
        return budgets.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month).ToList();
    }

    public async Task<Budget> AddOrUpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        var existing = (await _budgetRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(b => b.Year == budget.Year && b.Month == budget.Month && b.Category == budget.Category && b.MemberId == budget.MemberId);

        if (existing is not null)
        {
            existing.Amount = budget.Amount;
            existing.Notes = budget.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            await _budgetRepo.UpdateAsync(existing, cancellationToken);
            await _budgetRepo.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("更新预算: {Year}-{Month} {Category} = {Amount}", budget.Year, budget.Month, budget.Category, budget.Amount);
            return existing;
        }

        await _budgetRepo.AddAsync(budget, cancellationToken);
        await _budgetRepo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增预算: {Year}-{Month} {Category} = {Amount}", budget.Year, budget.Month, budget.Category, budget.Amount);
        return budget;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var budget = await _budgetRepo.GetByIdAsync(id, cancellationToken);
        if (budget is null) return;
        budget.IsDeleted = true;
        budget.UpdatedAt = DateTime.UtcNow;
        await _budgetRepo.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<(Budget Budget, BudgetStatus Status)>> GetMonthlyStatusAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var budgets = await _budgetRepo.GetAllAsync(cancellationToken);
        var monthBudgets = budgets.Where(b => b.Year == year && b.Month == month).ToList();

        var records = await _financeRepo.GetAllAsync(cancellationToken);
        var monthExpenses = records
            .Where(r => r.Type == FinanceType.Expense && r.RecordDate.HasValue && r.RecordDate.Value.Year == year && r.RecordDate.Value.Month == month)
            .ToList();

        var result = new List<(Budget, BudgetStatus)>();
        foreach (var budget in monthBudgets)
        {
            var actual = string.IsNullOrEmpty(budget.Category)
                ? monthExpenses.Sum(r => r.Amount)
                : monthExpenses.Where(r => r.Category == budget.Category).Sum(r => r.Amount);
            var remaining = budget.Amount - actual;
            var usagePercent = budget.Amount > 0 ? (actual / budget.Amount) * 100m : 0m;
            result.Add((budget, new BudgetStatus(budget.Amount, actual, remaining, usagePercent)));
        }
        return result;
    }
}
