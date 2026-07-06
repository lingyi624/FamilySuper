using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class HealthService : IHealthService
{
    private readonly IRepository<HealthRecord> _repo;
    private readonly ILogger<HealthService> _logger;

    public HealthService(IRepository<HealthRecord> repo, ILogger<HealthService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<HealthRecord>> GetRecordsAsync(long? memberId = null, HealthRecordType? type = null, CancellationToken cancellationToken = default)
    {
        var records = await _repo.GetAllAsync(cancellationToken);
        if (memberId is not null) records = records.Where(r => r.MemberId == memberId).ToList();
        if (type is not null) records = records.Where(r => r.RecordType == type).ToList();
        return records.OrderByDescending(r => r.RecordDate).ToList();
    }

    public async Task<HealthRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<HealthRecord> AddAsync(HealthRecord record, CancellationToken cancellationToken = default)
    {
        await _repo.AddAsync(record, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增健康档案: {Title} ({Type})", record.Title, record.RecordType);
        return record;
    }

    public async Task UpdateAsync(HealthRecord record, CancellationToken cancellationToken = default)
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
}
