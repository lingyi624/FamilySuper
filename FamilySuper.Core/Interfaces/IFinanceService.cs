using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

public interface IFinanceService
{
    Task<List<FinanceRecord>> GetRecordsAsync(FinanceType? type = null, string? category = null, CancellationToken cancellationToken = default);
    Task<FinanceRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<FinanceRecord> AddAsync(FinanceRecord record, CancellationToken cancellationToken = default);
    Task UpdateAsync(FinanceRecord record, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<FinanceSummary> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

public record FinanceSummary(decimal TotalIncome, decimal TotalExpense, decimal Balance, int RecordCount);
