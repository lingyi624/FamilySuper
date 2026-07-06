using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public record StudyReport(
    int TotalMinutes,
    int TotalSessions,
    Dictionary<string, int> MinutesBySubject,
    Dictionary<string, int> MinutesByDay,
    double CompletionRate,
    List<StudyLog> RecentLogs);

public interface IStudyService
{
    Task<List<StudyPlan>> GetPlansAsync(long? memberId = null, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<StudyPlan?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<StudyPlan> AddPlanAsync(StudyPlan plan, CancellationToken cancellationToken = default);
    Task UpdatePlanAsync(StudyPlan plan, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(long id, CancellationToken cancellationToken = default);

    Task<List<StudyLog>> GetLogsAsync(long? planId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<StudyLog> LogStudyAsync(StudyLog log, CancellationToken cancellationToken = default);

    Task<StudyReport> GenerateReportAsync(long? memberId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
