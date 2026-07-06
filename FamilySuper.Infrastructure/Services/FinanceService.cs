using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class FinanceService : IFinanceService
{
    private readonly IRepository<FinanceRecord> _repo;
    private readonly ILogger<FinanceService> _logger;

    public FinanceService(IRepository<FinanceRecord> repo, ILogger<FinanceService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<FinanceRecord>> GetRecordsAsync(FinanceType? type = null, string? category = null, CancellationToken cancellationToken = default)
    {
        var records = await _repo.GetAllAsync(cancellationToken);
        if (type is not null) records = records.Where(r => r.Type == type).ToList();
        if (!string.IsNullOrWhiteSpace(category)) records = records.Where(r => r.Category == category).ToList();
        return records.OrderByDescending(r => r.RecordDate).ToList();
    }

    public async Task<FinanceRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<FinanceRecord> AddAsync(FinanceRecord record, CancellationToken cancellationToken = default)
    {
        await _repo.AddAsync(record, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增财务记录: {Title} {Type} {Amount}", record.Title, record.Type, record.Amount);
        return record;
    }

    public async Task UpdateAsync(FinanceRecord record, CancellationToken cancellationToken = default)
    {
        record.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(record, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var record = await _repo.GetByIdAsync(id, cancellationToken);
        if (record is null) return;
        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task<FinanceSummary> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var records = await _repo.GetAllAsync(cancellationToken);
        if (startDate is not null) records = records.Where(r => r.RecordDate >= startDate).ToList();
        if (endDate is not null) records = records.Where(r => r.RecordDate <= endDate).ToList();

        var income = records.Where(r => r.Type == FinanceType.Income).Sum(r => r.Amount);
        var expense = records.Where(r => r.Type == FinanceType.Expense).Sum(r => r.Amount);
        return new FinanceSummary(income, expense, income - expense, records.Count);
    }
}
