using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class EducationService : IEducationService
{
    private readonly IRepository<EducationRecord> _repo;
    private readonly ILogger<EducationService> _logger;

    public EducationService(IRepository<EducationRecord> repo, ILogger<EducationService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<EducationRecord>> GetRecordsAsync(long? memberId = null, string? subject = null, CancellationToken cancellationToken = default)
    {
        var records = await _repo.GetAllAsync(cancellationToken);
        if (memberId is not null) records = records.Where(r => r.MemberId == memberId).ToList();
        if (!string.IsNullOrWhiteSpace(subject)) records = records.Where(r => r.Subject == subject).ToList();
        return records.OrderByDescending(r => r.RecordDate).ToList();
    }

    public async Task<EducationRecord> AddQaAsync(string question, string answer, string? subject = null, long? memberId = null, CancellationToken cancellationToken = default)
    {
        var record = new EducationRecord
        {
            Question = question,
            Answer = answer,
            Subject = subject,
            MemberId = memberId,
            RecordDate = DateTime.UtcNow
        };
        await _repo.AddAsync(record, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增辅导问答: {Subject} - {Question}", subject, question[..Math.Min(50, question.Length)]);
        return record;
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
