using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class MedicationService : IMedicationService
{
    private readonly IRepository<MedicationPlan> _planRepo;
    private readonly IRepository<MedicationRecord> _recordRepo;
    private readonly ILogger<MedicationService> _logger;

    public MedicationService(
        IRepository<MedicationPlan> planRepo,
        IRepository<MedicationRecord> recordRepo,
        ILogger<MedicationService> logger)
    {
        _planRepo = planRepo;
        _recordRepo = recordRepo;
        _logger = logger;
    }

    public async Task<List<MedicationPlan>> GetPlansAsync(long? memberId = null, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var plans = await _planRepo.GetAllAsync(cancellationToken);
        if (memberId is not null) plans = plans.Where(p => p.MemberId == memberId).ToList();
        if (activeOnly) plans = plans.Where(p => p.IsActive).ToList();
        return plans.OrderByDescending(p => p.StartDate).ToList();
    }

    public async Task<MedicationPlan?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _planRepo.GetByIdAsync(id, cancellationToken);

    public async Task<MedicationPlan> AddPlanAsync(MedicationPlan plan, CancellationToken cancellationToken = default)
    {
        await _planRepo.AddAsync(plan, cancellationToken);
        await _planRepo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增用药计划: {Drug} (成员 {MemberId})", plan.DrugName, plan.MemberId);
        return plan;
    }

    public async Task UpdatePlanAsync(MedicationPlan plan, CancellationToken cancellationToken = default)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        await _planRepo.UpdateAsync(plan, cancellationToken);
        await _planRepo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlanAsync(long id, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepo.GetByIdAsync(id, cancellationToken);
        if (plan is null) return;
        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;
        await _planRepo.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<MedicationRecord>> GetRecordsAsync(long planId, CancellationToken cancellationToken = default)
    {
        var records = await _recordRepo.GetAllAsync(cancellationToken);
        return records.Where(r => r.MedicationPlanId == planId)
            .OrderByDescending(r => r.TakenAt)
            .ToList();
    }

    public async Task<MedicationRecord> RecordDoseAsync(MedicationRecord record, CancellationToken cancellationToken = default)
    {
        await _recordRepo.AddAsync(record, cancellationToken);
        await _recordRepo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("记录服药: 计划 {PlanId}, 状态 {Status}", record.MedicationPlanId, record.Status);
        return record;
    }

    public async Task<List<MedicationPlan>> GetDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = new DateTime(now.Year, now.Month, now.Day);
        var plans = await _planRepo.GetAllAsync(cancellationToken);
        var activePlans = plans.Where(p => p.IsActive && p.StartDate <= now && (p.EndDate == null || p.EndDate >= today)).ToList();

        var result = new List<MedicationPlan>();
        foreach (var plan in activePlans)
        {
            var records = await _recordRepo.GetAllAsync(cancellationToken);
            var todayRecords = records.Where(r => r.MedicationPlanId == plan.Id && r.TakenAt >= today).ToList();
            if (todayRecords.Count < plan.TimesPerDay)
            {
                result.Add(plan);
            }
        }
        return result;
    }
}
