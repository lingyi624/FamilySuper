using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public interface IMedicationService
{
    Task<List<MedicationPlan>> GetPlansAsync(long? memberId = null, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<MedicationPlan?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<MedicationPlan> AddPlanAsync(MedicationPlan plan, CancellationToken cancellationToken = default);
    Task UpdatePlanAsync(MedicationPlan plan, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(long id, CancellationToken cancellationToken = default);
    Task<List<MedicationRecord>> GetRecordsAsync(long planId, CancellationToken cancellationToken = default);
    Task<MedicationRecord> RecordDoseAsync(MedicationRecord record, CancellationToken cancellationToken = default);
    Task<List<MedicationPlan>> GetDueRemindersAsync(CancellationToken cancellationToken = default);
}
