using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class StudyService : IStudyService
{
    private readonly IRepository<StudyPlan> _planRepo;
    private readonly IRepository<StudyLog> _logRepo;
    private readonly ILogger<StudyService> _logger;

    public StudyService(
        IRepository<StudyPlan> planRepo,
        IRepository<StudyLog> logRepo,
        ILogger<StudyService> logger)
    {
        _planRepo = planRepo;
        _logRepo = logRepo;
        _logger = logger;
    }

    public async Task<List<StudyPlan>> GetPlansAsync(long? memberId = null, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var plans = await _planRepo.GetAllAsync(cancellationToken);
        if (memberId is not null) plans = plans.Where(p => p.MemberId == memberId).ToList();
        if (activeOnly) plans = plans.Where(p => p.IsActive).ToList();
        return plans.OrderByDescending(p => p.StartDate).ToList();
    }

    public async Task<StudyPlan?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _planRepo.GetByIdAsync(id, cancellationToken);
    }

    public async Task<StudyPlan> AddPlanAsync(StudyPlan plan, CancellationToken cancellationToken = default)
    {
        await _planRepo.AddAsync(plan, cancellationToken);
        await _planRepo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增学习计划: {Subject} 成员#{MemberId}", plan.Subject, plan.MemberId);
        return plan;
    }

    public async Task UpdatePlanAsync(StudyPlan plan, CancellationToken cancellationToken = default)
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

    public async Task<List<StudyLog>> GetLogsAsync(long? planId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var logs = await _logRepo.GetAllAsync(cancellationToken);
        if (planId is not null) logs = logs.Where(l => l.StudyPlanId == planId).ToList();
        if (from is not null) logs = logs.Where(l => l.StudyDate >= from).ToList();
        if (to is not null) logs = logs.Where(l => l.StudyDate <= to).ToList();
        return logs.OrderByDescending(l => l.StudyDate).ToList();
    }

    public async Task<StudyLog> LogStudyAsync(StudyLog log, CancellationToken cancellationToken = default)
    {
        await _logRepo.AddAsync(log, cancellationToken);
        await _logRepo.SaveChangesAsync(cancellationToken);

        var plan = await _planRepo.GetByIdAsync(log.StudyPlanId, cancellationToken);
        if (plan is not null)
        {
            var total = await _logRepo.GetAllAsync(cancellationToken);
            var planMinutes = total.Where(l => l.StudyPlanId == log.StudyPlanId).Sum(l => l.DurationMinutes);
            var target = plan.TargetHoursPerWeek * 60;
            if (target > 0)
            {
                plan.ProgressPercent = Math.Min(100, (int)(planMinutes * 100m / target));
                plan.UpdatedAt = DateTime.UtcNow;
                await _planRepo.SaveChangesAsync(cancellationToken);
            }
        }
        return log;
    }

    public async Task<StudyReport> GenerateReportAsync(long? memberId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var logs = await _logRepo.GetAllAsync(cancellationToken);
        var filtered = logs.Where(l => l.StudyDate >= from && l.StudyDate <= to).ToList();
        if (memberId is not null) filtered = filtered.Where(l => l.MemberId == memberId).ToList();

        var bySubject = filtered
            .GroupBy(l => l.StudyPlanId)
            .Select(g =>
            {
                var plan = _planRepo.GetByIdAsync(g.Key, cancellationToken).GetAwaiter().GetResult();
                return new { Subject = plan?.Subject ?? "未知", Minutes = g.Sum(l => l.DurationMinutes) };
            })
            .ToDictionary(x => x.Subject, x => x.Minutes);

        var byDay = filtered
            .GroupBy(l => l.StudyDate.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Sum(l => l.DurationMinutes));

        var completionRate = filtered.Count == 0 ? 0 : filtered.Count(l => l.Completion == "completed") * 100.0 / filtered.Count;

        return new StudyReport(
            TotalMinutes: filtered.Sum(l => l.DurationMinutes),
            TotalSessions: filtered.Count,
            MinutesBySubject: bySubject,
            MinutesByDay: byDay,
            CompletionRate: completionRate,
            RecentLogs: filtered.Take(20).ToList());
    }
}
